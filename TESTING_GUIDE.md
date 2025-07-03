# Hướng dẫn kiểm tra chức năng Thùng rác

## Phần 1: Kiểm tra chức năng "Chuyển vào thùng rác"

1.  **Khởi động**: Chạy cả `FileSharingServer` và `FileSharingClient`.
2.  **Tải file lên**: Đăng nhập vào client, vào mục "Upload" và tải lên một file bất kỳ.
3.  **Xem file**: Chuyển qua mục "My Files" và xác nhận file vừa tải lên đã xuất hiện.
4.  **Xóa file**: Chuột phải vào file đó, chọn "Delete" từ menu ngữ cảnh. Một hộp thoại xác nhận sẽ hiện ra.
5.  **Xác nhận**: Nhấn "Yes". Bạn sẽ nhận được thông báo "File đã được chuyển vào thùng rác." và file sẽ biến mất khỏi danh sách "My Files".
6.  **Kiểm tra Thùng rác**:
    *   Chuyển qua mục "Trash Bin" trên thanh điều hướng.
    *   **Kết quả mong đợi**: File bạn vừa xóa phải xuất hiện trong danh sách, với thông tin về ngày xóa và thời gian còn lại trước khi bị xóa vĩnh viễn (khoảng 29-30 ngày).
7.  **Kiểm tra Database (Tùy chọn)**:
    *   Sử dụng một công cụ như "DB Browser for SQLite" để mở file `test.db` trong thư mục project của server.
    *   Mở bảng `files` và tìm file bạn vừa xóa.
    *   **Kết quả mong đợi**: Cột `status` của file phải là `TRASH` và cột `deleted_at` phải có giá trị là ngày giờ bạn vừa xóa.

## Phần 2: Kiểm tra chức năng "Tự động xóa sau 30 ngày"

Vì việc chờ 30 ngày là không thực tế, chúng ta sẽ giả lập điều này bằng cách sửa đổi CSDL.

1.  **Chuẩn bị**: Thực hiện các bước trong **Phần 1** để có một file trong thùng rác.
2.  **Sửa đổi Database**:
    *   Mở file `test.db` bằng "DB Browser for SQLite".
    *   Trong bảng `files`, tìm file đang ở trong thùng rác.
    *   Sửa giá trị trong cột `deleted_at` thành một ngày trong quá khứ, cách đây hơn 30 ngày (ví dụ: `2023-01-01 10:00:00`). Lưu lại thay đổi.
3.  **Khởi động lại Server**: Tắt và mở lại ứng dụng `FileSharingServer`.
4.  **Quan sát Console Server**:
    *   **Kết quả mong đợi**: Ngay sau khi khởi động, server console sẽ in ra các dòng log thông báo về việc chạy tác vụ dọn dẹp thùng rác, ví dụ: `Running scheduled trash cleanup...`, `Found 1 files to permanently delete from trash.`, và `Trash cleanup finished.`.
5.  **Kiểm tra Client**:
    *   Trong client, vào lại mục "Trash Bin".
    *   **Kết quả mong đợi**: File bạn đã sửa ngày xóa phải biến mất khỏi danh sách.
6.  **Kiểm tra lại Database và Thư mục**:
    *   Kiểm tra lại bảng `files` trong `test.db`. Record của file đó phải bị xóa hoàn toàn.
    *   Kiểm tra thư mục `uploads` trên server. File vật lý tương ứng cũng phải bị xóa.

## Phần 3: Kiểm tra "Phục hồi" và "Xóa vĩnh viễn"

1.  **Chuẩn bị**: Đảm bảo có ít nhất một file trong "Trash Bin".
2.  **Kiểm tra Phục hồi**:
    *   Trong "Trash Bin", nhấn nút "Phục hồi" ở dòng của file.
    *   **Kết quả mong đợi**: File biến mất khỏi thùng rác. Quay lại mục "My Files", file đó phải xuất hiện trở lại. Trong CSDL, `status` của file phải trở về `ACTIVE` và `deleted_at` phải là `NULL`.
3.  **Kiểm tra Xóa Vĩnh viễn**:
    *   Chuyển một file khác vào thùng rác.
    *   Trong "Trash Bin", nhấn nút "Xóa" (Xóa vĩnh viễn).
    *   **Kết quả mong đợi**: File biến mất khỏi thùng rác và không thể phục hồi. Cả record trong CSDL và file vật lý trên server đều bị xóa.
