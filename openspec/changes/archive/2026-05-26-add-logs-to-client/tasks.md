## 1. Client gRPC Adapter Observability

- [x] 1.1 Add OpenTelemetry error logging to shopping gRPC read failures and rethrow the original errors.
- [x] 1.2 Add OpenTelemetry error logging to market gRPC read failures and rethrow the original errors.
- [x] 1.3 Keep successful empty shopping and market responses mapped as empty results without error logs.

## 2. API Route Error Responses

- [x] 2.1 Return explicit error responses from shopping list API reads when propagated gRPC read failures prevent valid data responses.
- [x] 2.2 Return explicit error responses from market product API reads when propagated gRPC read failures prevent valid data responses.

## 3. Verification

- [x] 3.1 Add focused unit tests for adapter failure logging, propagation, and successful empty responses.
- [x] 3.2 Run the relevant client type check, format check and unit tests.
