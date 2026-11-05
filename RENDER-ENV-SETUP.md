# Render 環境變數設置指南

## 問題背景

ASP.NET Core 應用在 Render 上啟動時報錯：
```
System.InvalidOperationException: JWT Secret not configured
```

**原因**：配置值未從環境變數正確讀取。ASP.NET Core 使用特定的環境變數命名規則。

## ASP.NET Core 環境變數命名規則

在 `appsettings.json` 中的配置路徑使用冒號 `:` 作為分隔符：
```json
{
  "Jwt": {
    "Secret": "value"
  },
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Landlord": {
    "Email": "...",
    "PasswordHash": "..."
  }
}
```

在環境變數中，冒號 `:` 必須替換為 **雙下劃線 `__`**：

| appsettings.json 路徑 | 環境變數名稱 |
|----------------------|------------|
| `Jwt:Secret` | `Jwt__Secret` |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `Landlord:Email` | `Landlord__Email` |
| `Landlord:PasswordHash` | `Landlord__PasswordHash` |

## Render 環境變數設置步驟

### 1. 訪問 Render Dashboard

```
https://dashboard.render.com/
```

### 2. 選擇 Web Service

- 左側選單 → 您的 Web Service (例如 `housedemoll-backend`)
- 進入 Web Service 詳情頁面

### 3. 進入 Environment 標籤

```
Web Service 詳情 → Environment
```

### 4. 新增環境變數

點擊 **Add Environment Variable** 按鈕，逐個新增以下變數：

#### 變數 1: JWT Secret

| 欄位 | 值 |
|------|-----|
| **Name** | `Jwt__Secret` |
| **Value** | `your-super-secret-jwt-key-change-in-production-minimum-32-characters-long!` |

⚠️ **提示**：在生產環境中，應使用更複雜的隨機字符串。建議使用：
```bash
# 生成隨機密鑰
openssl rand -base64 32
```

#### 變數 2: Landlord Email

| 欄位 | 值 |
|------|-----|
| **Name** | `Landlord__Email` |
| **Value** | `landlord@housedemoll.com` |

#### 變數 3: Landlord Password Hash

| 欄位 | 值 |
|------|-----|
| **Name** | `Landlord__PasswordHash` |
| **Value** | `$2a$11$361iDHFu5B80.65OdgiqhOxRXoBihMl3MSDWznlRNk7c04hzHyiSS` |

⚠️ **說明**：這是 `demo123456` 的 bcrypt hash。登入時用此密碼。

#### 變數 4: ASP.NET Core Environment

| 欄位 | 值 |
|------|-----|
| **Name** | `ASPNETCORE_ENVIRONMENT` |
| **Value** | `Production` |

#### 變數 5: Database URL (如果未設置)

| 欄位 | 值 |
|------|-----|
| **Name** | `DATABASE_URL` |
| **Value** | `postgres://user:password@host:5432/dbname` |

**取得 Render PostgreSQL 連接字符串**：
1. Render Dashboard → PostgreSQL 實例
2. 複製 "Internal Database URL"
3. 貼入上方 **Value** 欄位

### 5. 驗證環境變數

確認所有 5 個變數都已新增：

```
☑️ Jwt__Secret
☑️ Landlord__Email  
☑️ Landlord__PasswordHash
☑️ ASPNETCORE_ENVIRONMENT
☑️ DATABASE_URL
```

### 6. 手動重新部署

1. 點擊 Web Service 頁面上的 **"Manual Deploy"** 按鈕
2. 選擇 **"Deploy latest commit"**
3. 等待 5-10 分鐘部署完成

## 驗證部署成功

### 檢查應用日誌

1. Render Dashboard → Web Service → **Logs** 標籤
2. 查找以下成功訊息：

```
✅ "Determining Project type" 
✅ "Successfully built application"
✅ "Listening on http://0.0.0.0:xxxxx"
✅ "Connected to database"
✅ "Application started"
```

### 測試 API Endpoint

```bash
curl https://housedemoll-backend.onrender.com/health
```

預期響應（HTTP 200 OK）：
```json
{
  "status": "healthy"
}
```

### 測試登入

```bash
curl -X POST https://housedemoll-backend.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"landlord@housedemoll.com","password":"demo123456"}'
```

預期響應：
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJ0eXAiOiJKV1QiLC...",
  "expiresInSeconds": 86400
}
```

## 常見問題排查

### ❌ "JWT Secret not configured"

**原因**：`Jwt__Secret` 環境變數未設置或名稱錯誤

**檢查**：
- 環境變數名稱是否為 `Jwt__Secret`（注意雙下劃線）？
- 值是否非空？
- 部署後是否等待了 5-10 分鐘？

### ❌ "Connection refused" (PostgreSQL)

**原因**：`DATABASE_URL` 無效或 PostgreSQL 服務未運行

**檢查**：
- PostgreSQL 實例是否在 Render 上運行？
- 連接字符串是否正確？
- 防火牆是否允許連接？

### ❌ "Unhandled exception" 

**原因**：可能是代碼問題或其他配置錯誤

**檢查**：
- 查看完整日誌堆棧跟蹤
- 搜索錯誤訊息
- 檢查 `Program.cs` 語法是否有誤

## 生產環境最佳實踐

### 🔐 安全建議

1. **JWT Secret**：使用隨機生成的強密鑰
   ```bash
   openssl rand -base64 32
   ```

2. **Landlord Password**：修改預設密碼
   - 使用 bcrypt 哈希新密碼
   - 更新 `Landlord__PasswordHash` 環境變數

3. **數據庫**：
   - 使用 Render 管理的 PostgreSQL
   - 啟用自動備份
   - 定期備份數據

### 📊 監控

1. 啟用 Render 日誌保留
2. 設置錯誤告警
3. 定期檢查應用健康狀態

## 相關文件

- [`RENDER-DEPLOYMENT.md`](./RENDER-DEPLOYMENT.md) - 完整 Render 部署指南
- [`PRODUCTION-DEPLOYMENT.md`](./PRODUCTION-DEPLOYMENT.md) - 生產部署策略
- [`docker-compose.yml`](./docker-compose.yml) - 本地開發配置

