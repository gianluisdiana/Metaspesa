from pathlib import Path

import grpc_tools
from grpc_tools import protoc


def main() -> int:
    project_root = Path(__file__).resolve().parents[1]
    src_dir = project_root / "src"
    proto_dir = src_dir / "infrastructure" / "grpc" / "protos"
    grpc_tools_proto_dir = Path(grpc_tools.__file__).resolve().parent / "_proto"

    proto_files = sorted(str(proto_file) for proto_file in proto_dir.glob("*.proto"))

    return protoc.main(
        [
            "grpc_tools.protoc",
            f"--proto_path={src_dir}",
            f"--proto_path={grpc_tools_proto_dir}",
            f"--python_out={src_dir}",
            f"--grpc_python_out={src_dir}",
            *proto_files,
        ]
    )


if __name__ == "__main__":
    raise SystemExit(main())
