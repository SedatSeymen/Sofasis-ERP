namespace Sofasis.Module.Services;

// Saf hesaplama (XPO/UI bağımsız) — FaturaD.ObjectSaving() tarafından çağrılır. Satış Faturası
// (gelecek faz) da AYNI arayüzü kullanacak (bkz. plan §"Satış ↔ Satın Alma simetrisi") — Alış'a
// özgü hiçbir şey içermez.
public interface IVatCalculator
{
    // kdvOrani/tevkifatOrani TAM SAYI YÜZDE olarak beklenir (20 = %20 — KDVTanim.KDVOrani/
    // TevkifatTanim.TevkifatOrani ile aynı format, /100 burada İÇERDE yapılır).
    // TevkifatTutar: KDVTutar'ın tevkifatOrani kadarlık payı — tevkifatOrani null/0 ise 0.
    // NetTutar: KDVHaricTutar + KDVTutar - TevkifatTutar (tedarikçiye ÖDENECEK net tutar; tevkifat
    // edilen KDV payı alıcı tarafından sorumlu sıfatıyla ayrıca beyan edilir, tedarikçiye ödenmez).
    FaturaSatirTutarlari Hesapla(decimal miktar, decimal birimFiyat, decimal kdvOrani, decimal? tevkifatOrani);
}

public readonly record struct FaturaSatirTutarlari(
    decimal KDVHaricTutar,
    decimal KDVTutar,
    decimal TevkifatTutar,
    decimal NetTutar);
