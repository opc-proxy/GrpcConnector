// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Core;
using System.Collections.Generic;

using OpcProxyClient; 
using OpcProxyCore;
using Newtonsoft.Json.Linq;
using NLog;


namespace OpcGrpcConnect
{
    public class HttpImpl : Http.HttpBase, IOPCconnect
    {

        private serviceManager _services;
        private Server server;
        public static Logger logger = LogManager.GetCurrentClassLogger();


        // Server side handler of the SayHello RPC
        public override Task<Response> ReadOpcNodes(ReadRequest request, ServerCallContext context)
        {
            List<string> names = new List<string>{};

            foreach( var name in request.Names){
                names.Add(name);
            }
            var temp = _services.readValueFromCache(names.ToArray());
            temp.Wait();
            var values = temp.Result;

            Response r = new Response();

            foreach(var variable in values){
                NodeValue val = new NodeValue();
                val.Name = variable.name;
                val.Type = variable.systemType;
                val.Value = variable.value.ToString();
                val.Timestamp = variable.timestamp.ToUniversalTime().ToString("o");
                val.ErrorMessage = variable.statusCode.ToString();
                val.IsError = !variable.success;
                r.Nodes.Add(val);
            }

            return Task.FromResult( r );
        }
        public override Task<Response> WriteOpcNode (WriteRequest request, ServerCallContext context){
            
            List<string> names = new List<string>{};
            List<object> values = new List<object>{};

            for( int k =0; k<request.Names.Count; k++){
                names.Add(request.Names[k]);
                values.Add(request.Values[k]);
            }
            var temp = _services.writeToOPCserver(names.ToArray(), values.ToArray() ) ;
            temp.Wait();
            var data = temp.Result;
            Response r = new Response();

            foreach(var variable in data){
                NodeValue val = new NodeValue();
                val.Name = variable.name;
                val.Type = "";
                val.Value = variable.value.ToString();
                val.Timestamp = DateTime.Now.ToUniversalTime().ToString("o");
                val.ErrorMessage = variable.statusCode.ToString();
                val.IsError = !variable.success;
                r.Nodes.Add(val);
            }

            return Task.FromResult( r );
        }


        /// <summary>
        /// Not needed here, does nothing
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        public void OnNotification(object obj, MonItemNotificationArgs args){

        }

        /// <summary>
        /// This is to get the pointer to the service manager and have access to
        /// all it methods. One needs to store this pointer to a local variable.
        /// </summary>
        /// <param name="serv">Pointer to the current service manager</param>
        public void setServiceManager( serviceManager serv){
            _services = serv;
        }

        /// <summary>
        /// Initialization. Everything that needs to be done for initializzation will be passed here.
        /// </summary>
        /// <param name="config">JSON configuration see Newtonsoft.Json for how to parse an object out of it</param>
        public void init(JObject config, CancellationTokenSource cts){
            grpcConf conf = config.ToObject<grpcConfWrapper>().gRPC;

            server = new Server
            {
                Services = { Http.BindService(this) },
                Ports = { new ServerPort(conf.host, conf.port, ServerCredentials.Insecure) }
            };
            
            server.Start();

            logger.Info("Listening on:  "+ conf.host + ":" + conf.port.ToString());

        }

        public  void clean(){
            server.ShutdownAsync().Wait();
            logger.Debug("HTTP GRPC server is down");
        }

    }

    public class grpcConfWrapper{
        public grpcConf gRPC {get;set;}

        public grpcConfWrapper(){
            gRPC = new grpcConf();
        }
    }

    public class grpcConf{
        public string host {get;set;}
        public int port {get; set;}

        public grpcConf(){
            host = "localhost";
            port = 5051;
        }
    }

}
