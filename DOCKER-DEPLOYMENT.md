# Docker 部署指南

## 🐳 快速開始

### 本地開發（使用 Docker Compose）

```bash
# 啟動 PostgreSQL + 後端
docker-compose up -d

# 查看日誌
docker-compose logs -f backend

# 停止服務
docker-compose down

# 清除數據（重置數據庫）
docker-compose down -v
```

後端 API 將在 `http://localhost:5088` 可用。

---

## 🏗️ 構建 Docker 映像

### 方式 1：本地構建

```bash
# 構建映像
docker build -t housedemo-backend:latest .

# 運行容器
docker run -d \
  --name housedemo-backend \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=your-db-host;Database=housedb;Username=houseadmin;Password=your-password" \
  -e Jwt__Secret="your-32-char-secret" \
  housedemo-backend:latest

# 查看日誌
docker logs -f housedemo-backend

# 停止容器
docker stop housedemo-backend
docker rm housedemo-backend
```

### 方式 2：使用 GitHub Container Registry

```bash
# 登入 GitHub Container Registry
docker login ghcr.io -u your-username -p your-token

# 構建並標記
docker build -t ghcr.io/leoluziyuan/housedemo-backend:latest .

# 推送
docker push ghcr.io/leoluziyuan/housedemo-backend:latest
```

---

## 🚀 部署到生產環境

### Docker Hub / GitHub Container Registry

```bash
# 構建用於生產的映像
docker build -t housedemo-backend:v1.0.0 .

# 標記
docker tag housedemo-backend:v1.0.0 ghcr.io/leoluziyuan/housedemo-backend:v1.0.0
docker tag housedemo-backend:v1.0.0 ghcr.io/leoluziyuan/housedemo-backend:latest

# 推送
docker push ghcr.io/leoluziyuan/housedemo-backend:v1.0.0
docker push ghcr.io/leoluziyuan/housedemo-backend:latest
```

### Kubernetes 部署（可選）

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: housedemo-backend
spec:
  replicas: 2
  selector:
    matchLabels:
      app: housedemo-backend
  template:
    metadata:
      labels:
        app: housedemo-backend
    spec:
      containers:
      - name: backend
        image: ghcr.io/leoluziyuan/housedemo-backend:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secrets
              key: connection-string
        - name: Jwt__Secret
          valueFrom:
            secretKeyRef:
              name: jwt-secrets
              key: secret
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

---

## 📝 環境變數

| 變數 | 說明 | 範例 |
|------|------|------|
| `ConnectionStrings__DefaultConnection` | 數據庫連接字符串 | `Host=postgres;Database=housedb;...` |
| `Jwt__Secret` | JWT 簽名密鑰（最少 32 字符） | `your-secret-key-here` |
| `Landlord__Email` | Landlord 郵箱 | `landlord@housedemoll.com` |
| `Landlord__PasswordHash` | BCrypt 密碼哈希 | `$2a$11$...` |
| `ASPNETCORE_URLS` | 服務 URL（容器內固定） | `http://+:8080` |

---

## ✅ 健康檢查

```bash
# 檢查容器是否健康
docker ps --filter name=housedemo-backend

# 手動測試健康端點
curl http://localhost:8080/health

# 查看完整日誌
docker logs housedemo-backend
```

---

## 🔧 故障排除

### 映像無法構建

```bash
# 清除構建快取
docker builder prune

# 重新構建
docker build --no-cache -t housedemo-backend:latest .
```

### 數據庫連接失敗

```bash
# 檢查 PostgreSQL 是否運行
docker-compose ps

# 查看 PostgreSQL 日誌
docker-compose logs postgres

# 重啟 PostgreSQL
docker-compose restart postgres
```

### 應用無法啟動

```bash
# 查看詳細日誌
docker logs -f housedemo-backend

# 進入容器調試
docker exec -it housedemo-backend /bin/bash
```

---

## 📦 映像大小優化

多階段 Dockerfile 設計已優化映像大小：
- **構建階段**：使用 SDK 映像（編譯代碼）
- **運行階段**：使用運行時映像（僅 ~200MB）
- **最終映像大小**：~350-400MB（包含依賴）

---

## 🔐 生產安全建議

1. **使用環境變數管理密鑰**（不在容器中硬編碼）
2. **啟用容器掃描**（檢測漏洞）
3. **使用非 root 用戶**（考慮 Dockerfile USER 指令）
4. **定期更新基礎映像**（MCR .NET 映像）
5. **實施容器日誌聚合**（ELK、Splunk 等）
6. **配置網絡策略**（僅允許必要的端口）
7. **啟用資源限制**（CPU、內存）

---

## 📚 相關文件

- `Dockerfile` — 容器映像構建配置
- `docker-compose.yml` — 本地開發環境
- `.dockerignore` — 排除構建中不需要的文件
- `.github/workflows/docker-build.yml` — 自動化 CI/CD 構建

