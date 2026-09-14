# gRPC Proto Generation

This folder contains the scraper's local copies of the gRPC `.proto` contracts and
the generated Python modules used by `GrpcProductRepository`. Identity uses the
REST API and therefore has no gRPC protobuf in this directory.

The service contracts must stay aligned with the server proto definitions. When a
server proto changes, copy the relevant contract updates into this folder, then
regenerate the Python files from the scraper project root:

```powershell
uv run python scripts/generate_grpc_protos.py
```

Commit both the updated `.proto` files and generated `*_pb2.py` /
`*_pb2_grpc.py` files in the same change.
