/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TekKayitParametreListController.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Tek satırlık singleton parametre ekranları (StokParametre/
 *                    CariHesapParametre/GenelParametre) için "Yeni"/"Sil"
 *                    action'larını kapatır.
 *                    Canlı testte bulundu (2026-08-22): AfterConstruction'daki
 *                    dedup mantığı (CancelEdit + Session.DropChanges + Reload)
 *                    yalnızca Session içindeki nesneyi değiştirir — View'ın
 *                    gösterdiği CurrentObject'i DEĞİŞTİREMEZ, bu yüzden "Yeni"
 *                    tıklanınca DetailView hâlâ boş/varsayılan bir nesne
 *                    gösteriyordu ve Kaydet'e basılınca TekKayitAnahtari unique
 *                    index ihlaliyle (23505) çöküyordu. DatabaseSeeder artık her
 *                    iki sınıf için de TAM 1 kayıt garanti ettiğinden (bkz.
 *                    SeedStokParametre/SeedCariHesapParametre, Jenerator varsayılan
 *                    seçili), kalıcı çözüm ikinci bir kayıt oluşturulmasına hiç
 *                    izin vermemek — kullanıcı yalnızca mevcut satırı düzenler.
 * ****************************************************************************
 */

using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Controllers.Process;

public class TekKayitParametreListController : ViewController<ListView>
{
    static readonly Type[] SingletonParametreTipleri = { typeof(StokParametre), typeof(CariHesapParametre), typeof(GenelParametre) };

    protected override void OnActivated()
    {
        base.OnActivated();
        if (Array.IndexOf(SingletonParametreTipleri, View.ObjectTypeInfo.Type) < 0) return;

        NewObjectViewController yeniController = Frame.GetController<NewObjectViewController>();
        if (yeniController != null) yeniController.NewObjectAction.Active["TekKayitParametre"] = false;

        DeleteObjectsViewController silController = Frame.GetController<DeleteObjectsViewController>();
        if (silController != null) silController.DeleteAction.Active["TekKayitParametre"] = false;
    }
}
