# Sınıf Header ve Audit Standardı

## Kaynak kodu header

Base klasöründeki mevcut sınıf adları aynen korunur.

Her yeni veya düzenlenen C# sınıf dosyasında proje header'ı bulunur:

- Proje
- Dosya Adı
- Oluşturma Tarihi
- Oluşturan
- Son Güncelleme
- Son Güncelleyen
- Açıklama

İlk oluşturma bilgileri geçmişten gelen dosyalarda değiştirilmez.

Son güncelleme bilgileri dosya değişikliğinde güncellenir; değerler elle uydurulmaz.

## İş nesnesi audit alanları

Uygun Base sınıfında aşağıdaki alanlar kullanılacaktır:

- `OlusturmaTarihi`
- `Olusturan`
- `SonGuncellemeTarihi`
- `SonGuncelleyen`

Bu alanların nasıl doldurulacağı XAF/XPO yaşam döngüsü ve mevcut Audit Trail yaklaşımıyla uyumlu tasarlanacaktır.

## Önemli

Kaynak dosyası header bilgileri ile veritabanındaki iş nesnesi audit bilgileri farklı amaçlara hizmet eder.

DevExpress/XAF/XPO framework sınıf ve API isimleri değiştirilmez.
