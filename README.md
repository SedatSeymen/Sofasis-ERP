# SofasisERP

Koltuk (mobilya) üretimi + Türk vergi/muhasebe mevzuatına uygun **ön muhasebe** uygulaması.
**Teknoloji:** .NET 10 · DevExpress XAF 26.1.3+ · XPO · Blazor Server · SQL Server.

## Başlarken (geliştirici / AI asistanı)

Önce şunları oku:
- **`CLAUDE.md`** — Proje kuralları ve konvansiyonlar (Claude Code otomatik okur).
- **`.github/copilot-instructions.md`** — Aynı içerik (GitHub Copilot okur).
- **`docs/`** — Ayrıntılı dokümanlar:
  - `00_Kod-Konvansiyonlari.md` — İsimlendirme, taban sınıflar (Guid PK), audit sekme kuralı, numaralandırma, XAF desenleri.
  - `01_Mimari-ve-Kararlar.md` — Mimari + kararlar (ADR).
  - `02_Mevcut-Proje-Analizi.md` — Şablon projenin analizi (tekrarlanmayacak hatalar).
  - `03_Yol-Haritasi.md` — Fazlı plan.
  - `04_Veri-Modeli.md` — İş nesneleri haritası.
  - `CHANGELOG.md` — Değişiklik günlüğü.

## Durum

Tasarım/dokümantasyon aşaması. Kod henüz yok. Sıradaki adım: DevExpress Solution Wizard ile boş XAF Blazor Server kabuğu (26.1.3 / .NET 10 / XPO / Integrated Security / ApplicationUser), ardından `docs/03_Yol-Haritasi.md`'deki Faz 0.
