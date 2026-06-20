# AVA Security — Brand & Naming (avasec)

**Product:** AVA Security  
**Project / repo folder:** `avasec`  
**Executable:** `AVASecurity.exe`  
**AppData:** `%AppData%\avasec\`  
**License prefix:** `AVA-`  
**AI engine:** AVA Mind  

## Tagline

- EN: *Smart protection. Faster performance.*
- VI: *Bảo vệ thông minh. Máy chạy mượt hơn.*

## Solution structure

```
avasec/
├── avasec.sln
├── avasec.ui/          # Desktop WPF
├── avasec.core/
├── avasec.server/      # Web portal (Node.js)
├── plugins/
└── Business/
```

## Run

```powershell
dotnet run --project avasec.ui
.\scripts\start-portal.ps1
```
