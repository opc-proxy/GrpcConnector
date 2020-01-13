# OPC-Proxy gRPC Connector Library

gRPC is a modern open source high performance RPC framework, initially 
developed at Google. It is very flexible and userfriendly, it can easily 
put in communication different services independently on the programming 
language used. For more information visit [grpc.io](https://grpc.io/).

As a comunication layer gRPC uses HTTP2, while it uses 
[protocol buffers](https://developers.google.com/protocol-buffers/)
as serialization/deserialization and Interface Definition Language.


A client can initiate ``Read`` request and a ``Write`` request. 
In future also a subscritpion to server push on variable change will be available.
The read/write request and response are defined in the proto-config-file that you can download 
from the [repository](https://github.com/opc-proxy/GrpcConnector/blob/master/opcGrpcConnect/opc.grpc.connect.proto),
you can use the proto-config to generate automatically the code needed for the comunication in almost
any language.


# Documentation

You find full documentation of the OPC-Proxy project at: [opc-proxy.readthedocs.io](https://opc-proxy.readthedocs.io/en/latest/) 


# Add it to your project with nuGet

```bash
dotnet add package opcProxy.GrpcConnector 
```

