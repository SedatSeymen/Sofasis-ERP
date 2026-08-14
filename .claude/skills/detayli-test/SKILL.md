---
name: detayli-test
description: Kullanıcı "detaylı test yap" (veya eşdeğeri — "kapsamlı test", "4 ajanla kontrol et") dediğinde tetiklenir. Geldiğimiz aşamayı (son özellik veya genel proje durumu) 4 paralel uzman ajanla değerlendirir — Netsis/Logo/Mikro fonksiyonel uzmanı + mali müşavir (mevzuat/muhasebe doğruluğu), kıdemli Endüstri Mühendisi (süreç/darboğaz), kıdemli Yazılım Mühendisi (hata/arıza/kod kalitesi), Sistem Analisti (mimari doğruluk/eksiklik). KULLAN when user asks for a detailed/thorough multi-persona review of current work.
---

# Detaylı Test — 4 Ajanlı Değerlendirme Turu (SofasisERP)

Bu, ADR-017/018'de kurulan "3-ajan değerlendirme turu" pratiğinin kalıcı, 4 ajanlı hale getirilmiş standart sürümüdür. Kullanıcı "detaylı test yap" dediğinde (ya da net bir kapsam belirtmeden "kontrol et" derse en son tamamlanan özelliği/oturumu kapsam kabul ederek) bu süreci uygula.

## Kapsamı belirle

- Kullanıcı bir modül/özellik belirtmişse onu kullan.
- Belirtmemişse: **bu oturumda tamamlanan en son özellik/değişiklik seti** (son `CHANGELOG.md` girdisi + ilgili kod) varsayılan kapsamdır. Kullanıcı "geldiğimiz aşamaya kadar" derse, kapsamı son özellikle SINIRLI TUTMA — ilgili modülün UÇTAN UCA mevcut durumunu (ör. Satın Alma: Talep→Teklif→Sipariş→İrsaliye→Fatura→İade zinciri + Dashboard) kapsa.

## 4 ajanı SPAWN et — TEK mesajda, PARALEL, subagent_type="general-purpose"

Her ajana **aynı bağlam özeti** (ne inşa edildi, hangi dosyalar, hangi ADR'ler) + **kendi persona talimatı** verilir. Her ajan promptuna şunlar MUTLAKA eklenir:
- **"Raporunu Türkçe yaz."** (bu proje kuralı — bkz. bellek `feedback_agent_turkce_rapor`)
- **Sadece kod okuma değil, gerçek doğrulama yap:** ilgili SQL tablolarını `sqlcmd` ile sorgula, iş kurallarını/servis kodunu satır satır oku, `docs/00`–`04` ve `CLAUDE.md`'deki kararlarla karşılaştır.
- **⚠ Canlı tarayıcı (Playwright) testi YAPMA.** Bu 4 ajan PARALEL çalışıyor ve tek bir paylaşılan tarayıcı oturumu var — paralel Playwright kullanımı çakışmaya yol açar (bkz. bellek `feedback_playwright_paralel_ajan_cakismasi`). Şüpheli bir davranış UI'da doğrulanması gerekiyorsa, bunu "canlı doğrulama önerisi" olarak rapor et; ana oturum (orkestratör) tüm ajanlar bittikten sonra SIRAYLA doğrular.
- Bulguları **önem sırasına göre** (Kritik / Yüksek / Orta / Düşük) ve dosya:satır referanslarıyla listelemesini iste.

### Ajan 1 — Netsis/Logo/Mikro Fonksiyonel Uzmanı + Mali Müşavir
Persona: Türkiye'de Netsis, Logo (Tiger), Mikro gibi ticari ERP'lerde 15+ yıl deneyimli fonksiyonel danışman VE aynı zamanda muhasebe/vergi mevzuatına hakim bir mali müşavir.
Kontrol alanı: KDV/tevkifat hesaplama doğruluğu, Borç/Alacak yönü (Tekdüzen Hesap Planı uyumu — bkz. `docs/01` ADR-017 açık kararındaki "Cari Borç/Alacak yönü" riski), fatura/irsaliye numaralandırma ve yasal belge bütünlüğü, İade akışının muhasebesel doğruluğu (ADR-018), üç ticari ERP'nin standart iş akışlarıyla (Talep→Teklif→Sipariş→İrsaliye→Fatura→İade) kıyas, eksik kalan mevzuat gereksinimleri (e-Belge, tevkifat kodları — henüz Faz 4/3 kapsamında olduğu bilinsin, eksiklik olarak değil roadmap notu olarak işaretlensin).

### Ajan 2 — Kıdemli Endüstri Mühendisi
Persona: Süreç tasarımı, iş akışı optimizasyonu ve kullanıcı verimliliğinde uzman kıdemli endüstri mühendisi.
Kontrol alanı: Uçtan uca sürecin (Talep→Teklif→Sipariş→İrsaliye→Fatura→İade) mantıksal akışı, gereksiz adımlar/tıklamalar, eksik onay/kontrol noktaları, kullanıcı hatası riskleri (ör. yanlış miktar/fiyat girişine karşı koruma), darboğaz olabilecek manuel adımlar, Dashboard KPI'larının gerçekten operasyonel karar almayı destekleyip desteklemediği.

### Ajan 3 — Kıdemli Yazılım Mühendisi
Persona: 20+ yıl deneyimli, kurumsal .NET/XAF sistemlerinde çalışmış kıdemli yazılım mühendisi.
Kontrol alanı: Kod kalitesi (CLAUDE.md §0.1 standartları), N+1 sorgu/darboğaz, transaction sınırları, hata yönetimi (yutulan exception, eksik `UserFriendlyException`), race condition/çift-tıklama riskleri, `[Indexed(Unique=true)]` gibi DB-seviyesi garantilerin gerçekten uygulanıp uygulanmadığı (bkz. `docs/01`'deki bilinen risk), ölü kod, kopya-yapıştır hatası (yanlış etiket/mesaj), test kapsamı yeterliliği (`dotnet test`).

### Ajan 4 — Sistem Analisti
Persona: Uçtan uca veri tutarlılığı ve mimari bütünlükten sorumlu kıdemli sistem analisti.
Kontrol alanı: Veri modelinin (`docs/04_Veri-Modeli.md`) gerçek kodla tutarlılığı, ADR'lerde (`docs/01`) alınan kararların fiilen uygulanıp uygulanmadığı, modüller arası entegrasyon noktalarının (Cari ayna kaydı, Stok hareket motoru, Sipariş durum makinesi) doğruluğu, Model.DesignedDiffs.xafml'de yapısal risk taşıyan (çift-render, eksik Removed-blok) ekranlar, dokümantasyon-kod sapmaları.

## Sonuçları sentezle ve uygula

1. Tüm ajanlar tamamlanınca (arka planda çalışıyorlarsa bildirim gelecek — BEKLE, sonuçları UYDURMA) bulguları TEK bir listede topla, mükerrer/çelişen bulguları birleştir/çöz.
2. Kritik ve Yüksek önemli bulguları kullanıcıya ÖZETLE önce sun (CLAUDE.md §0 "eleştirel ortak" ilkesi — sessizce uyma, gerekçeli görüş bildir).
3. Kullanıcı onayıyla (ya da açık yetki varsa doğrudan) düzeltmeleri uygula; her düzeltme sonrası `dotnet build`/`dotnet test`.
4. Ajanların "canlı doğrulama önerisi" işaretlediği şüpheli davranışları SIRAYLA (paralel değil) Playwright ile doğrula.
5. `docs/CHANGELOG.md`'ye Türkçe kayıt ekle (kök neden/ne yapıldı/doğrulama); gerekirse yeni bir ADR.
6. Test verisi üretildiyse SQL ile temizle, 0 satır doğrula.
