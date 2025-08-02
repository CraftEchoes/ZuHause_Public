# zuHause租好智慧租屋平台

<img width="424" height="134" alt="logo" src="https://github.com/user-attachments/assets/465ff64b-8b31-414b-b377-c20382a4bc11" />

##### 動機：
為提供能夠大幅降低繁瑣租屋流程的智慧租賃平台，依照功能角色分工，分為租客、房東、後台管理者身分進行開發，希望降低房客找房困難、房東的租屋管理，以及提高租屋媒合率，並包含以下功能：
1. 線上預約看屋，看房後使用平台線上簽署並完成租約
2. 針對租客量身打造的房屋演算系統，推送最理想的房源
3. 綁定房源的傢俱租賃系統，配合第三方支付，快速解決租客傢俱問題
4. 後台管理及儀表板，提供管理人員更細部的客制調整，搭配 Chart.js 將數據更直觀地視覺化

##### 使用技術：
1. 系統主要使用ASP.NET Core MVC，並搭配Bootstrap開發
2. SQL Server 關聯式資料庫進行開發
3. Git進行版本管理、多人協作
4. 前後端以Razor Page 與 ViewModel 進行溝通
5. 使用 DinkToPdf 套件產生租賃合約 PDF
6. Partial View 及 ViewComponent實作可重複區塊
7. 使用Entity Framework Core 操作 SQL Server資料庫

##### 主要負責以下模組：
1. 會員註冊及資料管理：使用 Cookie Authentication Scheme 及 Claims 完成註冊、登入登出、資料修改、房東與房客身分切換功能
2. 線上簽署合約模組：線上簽約流程設計，使用Canvas提供使用者線上完成簽署，並使用 DinkToPdf 提供使用者下載完整合約
3. 系統訊息與通知模組：系統自動發送訊息、後台訊息串接、收到訊息後動作管理等
4. 資料庫設計與正規化：針對自身開發功能模組，規劃相關資料表設計及進行資料庫第三正規化，並建立資料表結構與關聯

##### 專案負責頁面與功能
1. 首頁：
   輪播區、猜你喜歡房源推薦、房源搜尋
    <img width="1904" height="890" alt="image" src="https://github.com/user-attachments/assets/574fdf8e-fb31-4fc4-b76a-e3d434943bb5" />
3. 會員資料管理：
   個人資料管理、上傳大頭照、進行身分認證
    <img width="1919" height="940" alt="image" src="https://github.com/user-attachments/assets/33639730-0615-49b7-b5ef-a3b79c960611" />
5. 通知管理：
   系統公告、系統通知
    <img width="1919" height="938" alt="image" src="https://github.com/user-attachments/assets/6fe8e802-c027-405a-acb9-e932f5cfa1e4" />
7. 申請紀錄管理：
   申請看房、申請租賃紀錄與狀態
     <img width="1919" height="938" alt="image" src="https://github.com/user-attachments/assets/9202260f-3a7c-43ed-933c-6e4fbfd3ee31" />
9. 合約管理：
   已完成租賃各合約查看與管理
     <img width="1914" height="939" alt="image" src="https://github.com/user-attachments/assets/d1519f55-bda7-43a1-a73e-7e5295ff831e" />
11. 房源內頁申請租賃：
    房源詳細頁申請租賃表單
     <img width="1916" height="937" alt="image" src="https://github.com/user-attachments/assets/4b804315-b305-4d5e-98a1-93b4c1d6f283" />
13. 查看租賃合約：
    簽署合約後瀏覽合約及下載PDF合約檔案
    
    https://github.com/user-attachments/assets/c709df01-64d9-426c-947d-5adfd906f7c7

