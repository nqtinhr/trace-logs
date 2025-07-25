## 8 chỉ số cần thiết nhất cho WinForms .NET app

| Nhóm             | Tên metric                               | Mục đích chính                           | PromQL (dùng cho cả 2 job)                                                    |
| ---------------- | ---------------------------------------- | ---------------------------------------- | ----------------------------------------------------------------------------- |
| 🧠 Bộ nhớ        | `dotnet_total_memory_bytes`              | Tổng memory đang được cấp phát (.NET GC) | `dotnet_total_memory_bytes{job="winforms_app1"}`                              |
| ♻️ GC            | `dotnet_collection_count_total`          | Số lần GC mỗi generation                 | `rate(dotnet_collection_count_total{job="winforms_app1",generation="0"}[5m])` |
| ⚙️ CPU           | `system_runtime_cpu_usage`               | % CPU sử dụng                            | `system_runtime_cpu_usage{job="winforms_app1"}`                               |
| 🧵 Threads       | `process_num_threads`                    | Tổng số thread đang hoạt động            | `process_num_threads{job="winforms_app1"}`                                    |
| 💾 RAM           | `process_working_set_bytes`              | RAM thực tế app đang dùng                | `process_working_set_bytes{job="winforms_app1"}`                              |
| ⛓ ThreadPool     | `system_runtime_threadpool_thread_count` | Số lượng thread trong ThreadPool         | `system_runtime_threadpool_thread_count{job="winforms_app1"}`                 |
| 📦 Assembly      | `system_runtime_assembly_count`          | Số assembly được load                    | `system_runtime_assembly_count{job="winforms_app1"}`                          |
| 🌐 HTTP (nếu có) | `system_net_http_requests_failed_total`  | Tổng số request bị lỗi                   | `rate(system_net_http_requests_failed_total{job="winforms_app1"}[5m])`        |
