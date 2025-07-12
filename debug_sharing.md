# Debug Sharing Workflow

## Vấn đề:
- User không thể share file cho tài khoản khác
- Khi nhập share pass được generate ra bị lỗi ở MyFile và ShareView

## Test Steps:

### 1. Test File Sharing (User A share file to User B)

**User A (Owner):**
1. Login với User A
2. Navigate to MyFile view
3. Right-click một file -> chọn "Share"
4. Chọn permission (read/write)
5. Ghi lại share password được hiển thị

**User B (Receiver):**
1. Login với User B  
2. Navigate to ShareView
3. Click "Get File" button
4. Nhập share password từ User A
5. Check xem có file hiển thị trong ShareView không

### 2. Debug Points:

**Check Server Logs:**
- GET_FILE_INFO_BY_SHARE_PASS response
- AddFileReferenceAsync result
- LoadSharedFoldersAndFilesAsync response

**Check Database:**
```sql
-- Check files table after sharing
SELECT file_id, file_name, share_pass, is_shared, shared_file_path FROM files WHERE is_shared = 1;

-- Check files_share table after User B inputs share pass
SELECT * FROM files_share;
```

### 3. Possible Issues:

1. **ShareFileAsync fails at any step:**
   - GenerateAndGetSharePassAsync returns empty
   - DownloadFileDataAsync returns null
   - Decryption fails with owner password
   - Re-encryption fails 
   - UploadSharedVersionAsync fails

2. **GetFileInfoBySharePassAsync fails:**
   - Share pass không exist trong database
   - Query syntax error

3. **AddFileReferenceAsync fails:**
   - Database constraint violation
   - Permission issues

4. **LoadSharedFoldersAndFilesAsync doesn't load shared files:**
   - GetSharedFilesDetailed query issue
   - files_share table không có data

### 4. Fix Strategy:

**Check ShareFileAsync logs:**
```
[DEBUG] Starting client-side re-encryption for file: {fileName}
[DEBUG] Generated share password: {sharePass}
[DEBUG] Downloaded original file data: {originalEncryptedData.Length} bytes
[DEBUG] Decrypted file data: {plainData.Length} bytes
[DEBUG] Re-encrypted with share password: {sharedEncryptedData.Length} bytes
```

**Check GetItemsByPasswordAsync logs:**
```
[DEBUG] GetFileInfoFromSharePassAsync returned: fileId={fileId}, ownerId={ownerId}
[DEBUG] AddFileReferenceAsync result: {shareResult}
```

**Check Server responses:**
```
GET_FILE_INFO_BY_SHARE_PASS|{sharePass} -> should return: 200|{fileId}|{ownerId}
ADD_FILE_SHARE_ENTRY_WITH_PERMISSION|{fileId}|{userId}|{sharePass}|{permission} -> should return: 200|
``` 