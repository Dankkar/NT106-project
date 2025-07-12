# Hướng dẫn chuyển đổi từ xử lý File sang Folder

## Tổng quan thay đổi

Hệ thống đã được chuyển đổi từ xử lý theo file riêng lẻ sang xử lý theo folder để tổ chức tốt hơn và quản lý dữ liệu hiệu quả hơn.

## Thay đổi Database Schema

### Tables mới được tạo:

1. **folders** - Lưu thông tin các folder
   - `folder_id`: ID duy nhất của folder
   - `folder_name`: Tên folder
   - `owner_id`: ID của chủ sở hữu
   - `parent_folder_id`: ID folder cha (cho nested folders)
   - `folder_path`: Đường dẫn folder
   - `created_at`, `updated_at`: Thời gian tạo và cập nhật
   - `share_pass`: Mật khẩu chia sẻ
   - `is_shared`: Trạng thái chia sẻ
   - `status`: Trạng thái folder (ACTIVE/TRASH)

2. **files** - Lưu thông tin các file thuộc folder
   - `file_id`: ID duy nhất của file
   - `file_name`: Tên file
   - `folder_id`: ID folder chứa file này
   - `file_size`: Kích thước file
   - `file_type`: Loại file
   - `file_path`: Đường dẫn file tương đối trong folder
   - `file_hash`: Hash SHA256 của file

3. **folder_shares** - Quản lý chia sẻ folder
   - `folder_id`: ID folder được chia sẻ
   - `shared_with_user_id`: ID user được chia sẻ
   - `permission`: Quyền truy cập (read/write/admin)
   - `share_pass`: Mật khẩu chia sẻ

## Thay đổi Protocol

### Commands mới:

1. **UPLOAD_FOLDER**: Upload một folder (dưới dạng ZIP)
   ```
   UPLOAD_FOLDER|folderName|totalSize|ownerId|uploadTime
   ```

2. **LIST_FOLDERS**: Lấy danh sách folder của user
   ```
   LIST_FOLDERS|userId
   ```

### Backward Compatibility:

- Command `UPLOAD` cũ vẫn hoạt động nhưng sẽ tạo folder tạm thời cho từng file

## Cách sử dụng

### Client phải gửi folder dưới dạng ZIP:

1. Client nén folder thành file ZIP
2. Gửi command `UPLOAD_FOLDER|folderName|zipSize|userId|timestamp`
3. Gửi data ZIP qua NetworkStream
4. Server sẽ:
   - Tạo record folder trong database
   - Giải nén ZIP
   - Lưu từng file vào database với `folder_id` tương ứng
   - Xóa file ZIP tạm

### Lấy danh sách folder:

```csharp
// Gửi command
string command = $"LIST_FOLDERS|{userId}";

// Nhận response format:
// "200|folderId1:folderName1:createdAt1:isShared1;folderId2:folderName2:createdAt2:isShared2;"
```

## Lợi ích của việc chuyển đổi

1. **Tổ chức tốt hơn**: Files được nhóm theo folder
2. **Chia sẻ hiệu quả**: Chia sẻ cả folder thay vì từng file
3. **Nested folders**: Hỗ trợ thư mục con
4. **Quản lý permissions**: Kiểm soát quyền truy cập chi tiết
5. **Scalability**: Dễ mở rộng cho các tính năng mới

## Files đã được cập nhật

- `DatabaseHelper.cs`: Thêm method InitializeDatabaseAsync()
- `FolderService.cs`: Service mới xử lý folder operations
- `ProtocolHandler.cs`: Cập nhật để hỗ trợ folder commands
- `FolderMigration.sql`: Script tạo tables mới

## Lưu ý Migration

- Tables cũ (`activity_logs`, `files_share`) vẫn tồn tại để tương thích
- Có thể xóa sau khi chắc chắn hệ thống hoạt động ổn định
- Backup database trước khi migration production 