# Render 部署指南 — HouseDemo 後端

**平台:** Render.com（支持 Docker 容器化部署）  
**成本:** $7/月起（包含 PostgreSQL）  
**優點:** 免費自動 HTTPS、更快啟動、PostgreSQL 集成  
**文檔:** https://render.com/docs

---

## 快速對比

| 特性 | Heroku | Render |
|------|--------|--------|
| 免費層 | ❌ 已移除 | ✅ 可用（睡眠） |
| 自動 HTTPS | ✅ | ✅ |
| Docker 支持 | ✅ | ✅ |
| 啟動時間 | ~20s（冷啟） | ~10s（更快） |
| PostgreSQL | $50/月+ | $15/月起 |
| 成本/月 | $7+ | $7+ |
| 自動部署 | ✅ GitHub | ✅ GitHub |

---

## 前置準備

### 需要的工具/帳號

- ✅ GitHub 帳號（已有）
- ✅ Render 帳號（免費在 render.com 註冊）
- ✅ Docker Dockerfile 已準備（已存在）

### 驗證 Dockerfile

```bash
# 確認 Dockerfile 存在且有效
cat Dockerfile | head -20

# 確認 docker-compose.yml 也存在（本地開發用）
cat docker-compose.yml | head -10
```

---

## STEP 1: 在 Render 創建帳號

1. 訪問 https://render.com
2. 點擊 "Sign up"
3. 用 GitHub 帳號登入（推薦）或創建新帳號
4. 完成郵件驗證

**現在你在 Render Dashboard**

---

## STEP 2: 創建 PostgreSQL 數據庫

### 2.1 在 Render 創建 Postgres 實例

1. Dashboard 首頁 → 點擊 "+ New"
2. 選擇 "PostgreSQL"
3. 填寫配置：

```
Name:                housedemoll-db
Region:              Singapore (or closest to you)
PostgreSQL Version:  15
Plan:                Standard ($15/month, optional)
```

4. 點擊 "Create Database"
5. **記下連接信息**（會在環境變數中用到）：

```
Internal Database URL: postgres://user:pass@host:5432/dbname
External Database URL: 同上（用於外部連接）
```

### 2.2 驗證數據庫連接

等待 2-3 分鐘數據庫啟動，然後：

```bash
# 使用提供的連接字符串測試
psql "postgres://user:pass@host:5432/dbname"

# 在 psql 提示中執行：
\dt           # 查看表（應為空，還未運行遷移）
\q            # 退出
```

---

## STEP 3: 在 GitHub 配置部署

### 3.1 添加 Render 部署密鑰（可選但推薦）

```bash
# 生成 SSH 密鑰給 Render（可選，用於拉取私有倉庫）
ssh-keygen -t ed25519 -f ~/.ssh/render_deploy -N ""

# 顯示公鑰
cat ~/.ssh/render_deploy.pub
```

將公鑰添加到 GitHub：
1. GitHub Settings → SSH and GPG keys
2. 點擊 "New SSH key"
3. 粘貼公鑰，標題：`Render Deployment`

### 3.2 在 GitHub 創建個人訪問令牌（推薦）

1. GitHub Settings → Developer settings → Personal access tokens
2. 點擊 "Tokens (classic)"
3. 點擊 "Generate new token (classic)"
4. 權限：勾選 `repo`（完整訪問倉庫）
5. 複製令牌，保存到安全地方

---

## STEP 4: 在 Render 創建 Web Service

### 4.1 創建新 Web Service

1. Render Dashboard → 點擊 "+ New"
2. 選擇 "Web Service"
3. 連接 GitHub 倉庫：

```
GitHub Account:      LeoLuZiYuan（或你的帳號）
Repository:          HouseDemo
Branch:              master
```

4. 填寫 Web Service 配置：

```
Name:                housedemoll-backend
Region:              Singapore (or Singapore - Near)
Runtime:             Docker

Build Command:       (留空，使用 Dockerfile)
Start Command:       (留空，使用 Dockerfile 的 CMD)

Plan:                Standard ($7/month minimum)
```

5. 點擊 "Advanced"，設置環境變數

### 4.2 設置環境變數

在 "Advanced" 中找到 "Environment" 部分，添加以下變數：

```
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=postgres://user:pass@host:5432/dbname  (從 STEP 2 複製)

JWT_SECRET=<generate-with-openssl-rand-base64-32>
ADMIN_EMAIL=admin@housedemoll.com
ADMIN_PASSWORD=your-secure-password-here

ASPNETCORE_URLS=http://+:10000
ASPNETCORE_HTTPS_PORT=443
```

**⚠️ 重要環境變數說明：**

| 變數 | 值 | 說明 |
|------|-----|------|
| `ASPNETCORE_ENVIRONMENT` | Production | 啟用生產模式 |
| `DATABASE_URL` | postgres://... | PostgreSQL 連接字符串 |
| `JWT_SECRET` | 隨機 32+ 字符 | JWT 簽名密鑰 |
| `ADMIN_EMAIL` | admin@... | 房東登入郵箱 |
| `ADMIN_PASSWORD` | 強密碼 | 房東登入密碼 |
| `ASPNETCORE_URLS` | http://+:10000 | 監聽端口 |

**生成 JWT_SECRET：**

```bash
openssl rand -base64 32
# 輸出示例：
# aBcDeFgHiJkLmNoPqRsTuVwXyZaBcDeFgHiJkLmNo=
# 複製這個值到 JWT_SECRET
```

### 4.3 配置健康檢查

在 "Advanced" 中，找到 "Health Check" 部分：

```
Health Check Path:  /api/properties
Health Check Port:  10000
Initial Delay:      60 (seconds)
Interval:          300 (seconds)
```

這會讓 Render 定期檢查後端是否在線。

### 4.4 創建 Service

點擊 "Create Web Service"

**預期行為：**
- Render 開始構建 Docker 映像
- 推送至內部倉庫
- 容器啟動
- 自動分配域名，例如：`https://housedemoll-backend.onrender.com`

**監控部署進度：**
在 Service 頁面查看 "Logs" 標籤

```
正在構建...
Step 1/10: FROM mcr.microsoft.com/dotnet/sdk:8.0
...
Successfully built image
Deploying...
Container started, listening on port 10000
✅ Service is live
```

---

## STEP 5: 測試後端部署

### 5.1 等待服務啟動

Render 需要 2-3 分鐘才能完成首次部署。

在 Render Dashboard 查看：
- Service 名稱下應顯示綠色 "Live" 標籤
- URL 類似：`https://housedemoll-backend.onrender.com`

### 5.2 驗證 API 端點

```bash
# 替換 YOUR_SERVICE_URL 為實際的 Render 服務 URL
SERVICE_URL="https://housedemoll-backend.onrender.com"

# 測試 GET 端點
curl "$SERVICE_URL/api/properties"

# 預期響應：
# [{"id":"...","name":"...","monthlyRent":"..."}]

# 測試健康檢查
curl "$SERVICE_URL/api/properties" -v
# 應返回 200 OK
```

### 5.3 驗證數據庫連接

```bash
# 查看 Render 日誌中是否有數據庫連接錯誤
# Render Dashboard → Service → Logs

# 應看到類似：
# Database migration completed successfully
# Listening on http://+:10000
```

---

## STEP 6: 配置自動部署

### 6.1 啟用 GitHub 自動部署

在 Render Web Service 頁面：

1. 左側菜單 → "Settings"
2. 找到 "Deploy Hook" 或 "GitHub Integration"
3. 應已自動連接（因為你用 GitHub 登入）

### 6.2 配置自動部署觸發

確保以下設置已配置：

```
Repository:   LeoLuZiYuan/HouseDemo
Branch:       master
Auto-Deploy:  On (enabled)
```

**現在每次 `git push origin master` 時：**
1. GitHub 通知 Render
2. Render 自動拉取最新代碼
3. 重新構建 Docker 映像
4. 重新部署到生產環境
5. 無縫切換（不中斷服務）

### 6.3 部署通知（可選）

1. Render Dashboard → Service Settings
2. 找到 "Notifications"
3. 添加 Email 通知部署成功/失敗

---

## STEP 7: 更新前端配置

### 7.1 更新 API 端點

編輯 `src/frontend/src/composables/usePropertyApi.js`：

```javascript
// 原來的：
const apiBase = import.meta.env.VITE_API_BASE || 'http://localhost:5088';

// 改為 Render 域名：
const apiBase = import.meta.env.VITE_API_BASE || 
                'https://housedemoll-backend.onrender.com';
```

或使用環境變數 `.env.production`：

```
VITE_API_BASE=https://housedemoll-backend.onrender.com
```

### 7.2 提交並推送

```bash
git add src/frontend/src/composables/usePropertyApi.js
git commit -m "feat: update API endpoint to Render backend

Updates usePropertyApi.js to point to Render-hosted backend
instead of Heroku.

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"

git push origin master
```

GitHub Actions 自動觸發：
- 運行 E2E 測試（針對 Render 後端）
- 構建前端
- 部署到 GitHub Pages

---

## STEP 8: 完整驗證

### 8.1 檢查清單

```bash
□ Render Web Service 已創建並 "Live"
□ PostgreSQL 數據庫已啟動
□ 環境變數已配置
□ API 端點可訪問：
  curl https://housedemoll-backend.onrender.com/api/properties
  
□ 前端已部署到 GitHub Pages：
  https://leoluziyuan.github.io/HouseDemo/
  
□ 前端能成功調用 Render 後端 API
  • DevTools Network 標籤顯示 api/properties 200 OK
  
□ 登入功能正常：
  • 導航到 /login
  • 輸入環境變數中的憑據
  • 成功重定向到 /admin
  
□ 後台 CRUD 功能驗證：
  ✓ 創建新房型
  ✓ 編輯現有房型
  ✓ 刪除房型
  ✓ 公開網站實時反映更改

□ 性能測試：
  • 首次加載 < 3 秒
  • 後續導航 < 1 秒
  • 無控制台錯誤
```

### 8.2 詳細測試腳本

```bash
#!/bin/bash
# test-render-deployment.sh

BACKEND="https://housedemoll-backend.onrender.com"
FRONTEND="https://leoluziyuan.github.io/HouseDemo"

echo "🔍 Testing Render Backend..."
curl -s "$BACKEND/api/properties" | jq '.[0]'

echo "🔍 Testing Frontend..."
curl -s "$FRONTEND" | grep -q "HouseDemo" && echo "✅ Frontend OK" || echo "❌ Frontend Failed"

echo "✅ All checks passed!"
```

運行：`bash test-render-deployment.sh`

---

## STEP 9: 監控與日誌

### 9.1 實時日誌

Render Dashboard → Service → Logs

```
# 查看實時日誌（自動更新）
# 按 Ctrl+C 停止

# 常見日誌消息：
app[web.1]: Listening on http://+:10000
app[web.1]: Database migration completed
app[web.1]: Started property service
```

### 9.2 部署歷史

Render Dashboard → Service → "Deployments" 標籤

查看所有部署歷史和回滾選項

### 9.3 設置告警

1. Service Settings → "Notifications"
2. 添加郵箱用於收到：
   - 部署成功通知
   - 部署失敗告警
   - 服務宕機告警

---

## 常見問題與解決方案

### 問題 1: 容器啟動失敗

**症狀:** 日誌顯示 "Port 10000 already in use" 或 "Build failed"

**解決方案:**
1. 檢查 Dockerfile 中的端口配置
2. 確認 ASPNETCORE_URLS 環境變數設置為 `http://+:10000`
3. 點擊 "Manual Deploy" 重新部署

### 問題 2: 數據庫連接超時

**症狀:** 日誌顯示 "Failed to connect to database"

**解決方案:**
1. 驗證 DATABASE_URL 格式正確
2. 確認 PostgreSQL 實例已啟動（Render Dashboard 中顯示綠色）
3. 等待 2-3 分鐘，可能數據庫仍在初始化
4. 檢查防火牆設置（Render 默認允許所有連接）

### 問題 3: CORS 錯誤

**症狀:** 前端發出的 API 請求被 CORS 拒絕

**解決方案:**

編輯 `src/backend/HouseDemo.Backend/Program.cs`，確保有 CORS 配置：

```csharp
app.UseCors(policy => 
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader()
);
```

提交並推送：

```bash
git add src/backend/HouseDemo.Backend/Program.cs
git commit -m "fix: enable CORS for Render deployment"
git push origin master
```

Render 自動重新部署。

### 問題 4: 免費層服務進入休眠

**症狀:** 首次訪問時響應時間 30+ 秒

**原因:** Render 免費層在 15 分鐘無活動後自動暫停

**解決方案:**
- 升級到付費計畫（Standard $7/月）以保持 24/7 活動
- 或使用 cron 服務定期 ping 保持活動

### 問題 5: 容器大小過大

**症狀:** 構建超時或部署失敗

**解決方案:**
確認 Dockerfile 使用多階段構建（已正確配置）：
```dockerfile
# 階段 1: 構建
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
...

# 階段 2: 運行時
FROM mcr.microsoft.com/dotnet/aspnet:8.0
...
```

---

## 與 Heroku 的遷移比較

如果之前部署到 Heroku，以下是遷移檢查清單：

```bash
□ Render Web Service 已創建
□ PostgreSQL 數據庫已創建
□ 環境變數已複製：
  - ASPNETCORE_ENVIRONMENT
  - DATABASE_URL（新的 Render 連接字符串）
  - JWT_SECRET（保持相同，無需重新生成）
  - ADMIN_EMAIL、ADMIN_PASSWORD

□ 代碼中的 API 端點已更新（Heroku → Render）
□ GitHub 自動部署已配置
□ DNS/域名已指向新後端（如適用）
□ 從 Heroku 匯出舊數據庫（如需要）：
  
  # 在 Heroku 備份
  heroku pg:backups:capture --app housedemoll-backend
  heroku pg:backups:download
  
  # 在 Render 恢復
  psql "postgres://user:pass@host:5432/dbname" < latest.dump
```

---

## 成本分析

| 服務 | 方案 | 成本/月 | 說明 |
|------|------|--------|------|
| Render Web Service | Standard | $7 | 後端容器，保證在線 |
| Render PostgreSQL | Starter | $15 | 10GB 存儲，足夠小型應用 |
| GitHub Pages | Free | $0 | 前端靜態托管 |
| **總計** | | **$22/月** | 生產環境完整堆棧 |

**成本優化：**
- 使用 Render 免費層（$0，但需應付冷啟動）
- PostgreSQL 也有免費層（30 天有效期）
- GitHub Pages 永遠免費

---

## 下一步

### 立即部署

1. 訪問 https://render.com 創建帳號
2. 按照 STEP 1-8 創建 Web Service 和數據庫
3. 驗證端點是否工作
4. 更新前端 API 端點
5. 提交並推送代碼

### 監控和維護

- 定期檢查 Render Dashboard 的服務狀態
- 設置部署通知
- 備份數據庫
- 監控成本和使用情況

### 未來優化

- 配置 CDN（Cloudflare）加速前端
- 設置自動數據庫備份
- 添加性能監控和告警
- 計劃 V2 功能（多房東、圖片上傳等）

---

## 參考資源

- Render 官方文檔: https://render.com/docs
- Docker 部署文檔: DOCKER-DEPLOYMENT.md
- Heroku 部署文檔: PRODUCTION-DEPLOYMENT.md
- .NET 部署指南: https://learn.microsoft.com/aspnet/core/host-and-deploy

---

**最後更新:** 2026-11-05  
**狀態:** ✅ Render 部署就緒  
**維護者:** @LeoLuZiYuan
