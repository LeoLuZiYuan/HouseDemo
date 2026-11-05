-- Seed properties table from rooms.json
-- This data is migrated from the frontend static data source to persistent PostgreSQL storage

INSERT INTO properties (name, area, monthly_rent, short_desc, description, features, cover_image, images)
VALUES 
  (
    '永安捷運大套房',
    '大套房（5+1樓）',
    'NT$12,800',
    '永安捷運站步行5分鐘，四號公園第一排，陽台直面樹海，5/15起租。',
    '新北市中和區永貞路，5+1樓大套房附大陽台，一層僅2間套房，安靜單純。坐擁四號公園第一排永久視野，非巷弄環境，陽台即可欣賞大片樹海。格局方正採光佳，房內設備齊全，生活機能優異，連鎖店林立。中正橋、福和橋、永福橋三橋直達台北市，對外交通便捷。免服務費，5/15起租。',
    '["捷運步行5分鐘", "公園第一排景觀", "大陽台", "雙人掀床＋床墊", "木衣櫃＋書桌辦公椅", "一級能效雙門冰箱", "變頻分離式冷暖氣", "獨立洗衣機", "LED大電視", "電磁爐＋微波爐", "天然瓦斯熱水器", "含網路(600M)＋第四台", "含水費＋瓦斯費", "電費一度5元"]',
    'https://res.cloudinary.com/dtreubqr3/image/upload/v1775920679/HouseDemo/Room1/S__45940796_0_tha4t0.jpg',
    '["https://res.cloudinary.com/dtreubqr3/image/upload/v1775920678/HouseDemo/Room1/S__45940811_0_npbzkq.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920671/HouseDemo/Room1/S__45940801_0_o1meyx.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920670/HouseDemo/Room1/S__45940800_0_izce9q.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920670/HouseDemo/Room1/S__45940799_0_drpqih.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920669/HouseDemo/Room1/S__45940798_0_s6tvs5.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920669/HouseDemo/Room1/S__45940797_0_gplqqy.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775919397/HouseDemo/Room1/S__45940810_0_tdwxzt.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920672/HouseDemo/Room1/S__45940802_0_uy3msy.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920674/HouseDemo/Room1/S__45940805_0_iwydtu.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920674/HouseDemo/Room1/S__45940804_0_rrh7ek.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920673/HouseDemo/Room1/S__45940803_0_zrhwol.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920675/HouseDemo/Room1/S__45940807_0_qeszk0.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920676/HouseDemo/Room1/S__45940808_0_oit1u1.jpg", "https://res.cloudinary.com/dtreubqr3/image/upload/v1775920677/HouseDemo/Room1/S__45940809_0_hptscp.jpg"]'
  ),
  (
    '城市輕旅套房',
    '20坪',
    'NT$28,000',
    '位於市中心，步行5分鐘即達捷運站，高樓視野盡收城市夜景。',
    '城市輕旅套房坐落於台北市核心地帶，交通便捷，周邊餐廳、便利商店、百貨公司一應俱全。房間採用現代簡約風格設計，大片落地窗將城市天際線盡收眼底，夜晚燈火璀璨，令人心曠神怡。套房內配備舒適的Queen Size雙人床、獨立衣帽間及精品衛浴，適合商務出差或都市短旅的旅客入住。',
    '["免費WiFi", "冷暖氣", "Queen Size雙人床", "近捷運", "高樓城市景觀", "獨立衛浴", "免費盥洗用品", "液晶電視"]',
    'https://picsum.photos/seed/city-suite-cover/800/600',
    '["https://picsum.photos/seed/city-suite-1/800/600", "https://picsum.photos/seed/city-suite-2/800/600", "https://picsum.photos/seed/city-suite-3/800/600"]'
  ),
  (
    '溫馨親子四人房',
    '35坪',
    'NT$45,000',
    '寬敞明亮的親子空間，附設廚房設備，鄰近公園與親子遊樂設施。',
    '溫馨親子四人房專為家庭旅客精心設計，空間寬敞達35坪，提供一張King Size大床及一組上下鋪床架，輕鬆容納四位家庭成員。房內設有獨立廚房區，配備電磁爐、電鍋與基本餐具，讓家長可以為孩子準備熟悉的料理。距離鄰近公園步行只需3分鐘，假日帶孩子出門踏青十分方便。整體以暖色調木質裝潢為主，營造溫馨居家感。',
    '["免費WiFi", "冷暖氣", "King Size大床", "上下鋪", "獨立廚房", "近公園", "兒童友善", "洗衣機", "嬰兒床（需預訂）"]',
    'https://picsum.photos/seed/family-room-cover/800/600',
    '["https://picsum.photos/seed/family-room-1/800/600", "https://picsum.photos/seed/family-room-2/800/600", "https://picsum.photos/seed/family-room-3/800/600"]'
  ),
  (
    '山景度假木屋',
    '28坪',
    'NT$36,000',
    '原木裝潢搭配私人露台，醒來即是翠綠山景，體驗山林度假的寧靜。',
    '山景度假木屋座落於青山翠谷之間，採用天然台灣檜木打造室內裝潢，散發清新木香，讓旅客徹底從都市壓力中解脫。私人露台直面連綿山脈，清晨享用咖啡、傍晚觀賞日落，皆是無可取代的體驗。木屋提供一張頂級彈簧雙人床與手工製棉被，確保旅客獲得最佳睡眠品質。屋外設有專屬停車位，自駕前來十分方便。',
    '["免費WiFi", "冷暖氣", "頂級彈簧雙人床", "私人露台", "森林山景", "停車位", "原木裝潢", "咖啡機", "BBQ設備（需預約）"]',
    'https://picsum.photos/seed/mountain-cabin-cover/800/600',
    '["https://picsum.photos/seed/mountain-cabin-1/800/600", "https://picsum.photos/seed/mountain-cabin-2/800/600", "https://picsum.photos/seed/mountain-cabin-3/800/600"]'
  );
