# HouseDemo — Copilot 專案指引

## 專案概述
純前端靜態租屋介紹網站，Airbnb 風格。無後端、無資料庫、無金流、無登入功能。

## 技術棧
- **Vue 3** — Composition API，一律使用 `<script setup>` 語法
- **Vite 5** — 搭配 `@tailwindcss/vite` 插件
- **Tailwind CSS v4** — CSS 使用 `@import "tailwindcss"`，禁止使用 v3 的 `@tailwind base/components/utilities`
- **Vue Router 4** — 使用 `createWebHashHistory`（GitHub Pages 相容）
- **JavaScript**（非 TypeScript）

## 資料來源
- 所有房型資料來自 `src/data/rooms.json`，不串接任何 API
- 圖片使用 Cloudinary 外部 URL 或 `picsum.photos/seed/{id}/800/600` 佔位圖
- 禁止將圖片檔案加入版控（`.gitignore` 已排除 `public/images/`）

## rooms.json 欄位規範
```json
{
  "id": "kebab-case-id",
  "name": "房型名稱",
  "area": "坪數或描述",
  "monthlyRent": "NT$xx,xxx",
  "shortDesc": "一行簡述，顯示於卡片",
  "description": "完整說明文字",
  "features": ["標籤1", "標籤2"],
  "coverImage": "https://...",
  "images": ["https://...", "https://..."]
}
```
禁止使用 `pricePerNight`，價格欄位統一為 `monthlyRent`。

## 元件結構
```
src/components/
  AppHeader.vue      — 頂部導覽列
  RoomCard.vue       — 首頁房型卡片
  ImageGallery.vue   — 圖片輪播（主圖 + 縮圖列）
src/views/
  HomeView.vue       — 房型列表頁（/）
  RoomDetailView.vue — 房型詳情頁（/room/:id）
```

## 程式碼慣例
- 新增元件時使用 `defineProps` 宣告 props，必須標示 `required`
- 圖片一律加 `loading="lazy"`
- 路由跳轉使用 `RouterLink`，禁止直接操作 `window.location`
- `computed` 取代 `methods` 做資料衍生

## Git Commit 格式
使用 Conventional Commits：
- `feat:` 新功能
- `fix:` 修正問題
- `ci:` CI/CD 設定
- `init:` 初始建立

## 部署
- 平台：GitHub Pages
- 自動部署：`.github/workflows/deploy.yml`（push to master 觸發）
- Custom domain：`housedemoll.com`（CNAME 在 `public/CNAME`）
- Node.js CI 版本：22
- `vite.config.js` 的 `base` 設為 `'/'`（使用 custom domain）
