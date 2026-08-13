using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FileSystemData.BusinessObjects;
using System.IO;
using DevExpress.Xpo.Metadata;
using System.Drawing;
using Microsoft.Extensions.Caching.Memory;


namespace Sofasis.Module.BusinessObjects;

public class ColorValueConverter : ValueConverter
{
    public override object ConvertFromStorageType(object obj)
    {
        if (obj == null)
        {
            return null;
        }
        return Color.FromArgb((Int32)obj);
    }
    public override object ConvertToStorageType(object obj)
    {
        if (obj == null)
        {
            return null;
        }
        return ((Color)obj).ToArgb();
    }
    public override Type StorageType
    {
        get { return typeof(Int32); }
    }
}
public static class Helper
{

    // #RRGGBB
    public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    // RGB(R, G, B)
    public static string ToRgbString(this Color c) => $"RGB({c.R}, {c.G}, {c.B})";

    // #RRGGBBAA
    public static string ToHexaString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";

    private static double ToProportion(byte b) => b / (double)Byte.MaxValue;

    // RGBA(R, G, B, A)
    public static string ToRgbaString(this Color c) => $"RGBA({c.R}, {c.G}, {c.B}, {ToProportion(c.A):N2})";
    public static string ToTitleCase(string value)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
    }
    public static string SayiyiYaziyaCevir(string sayı)
    {
        const int max_basamak_say = 18;

        string[] birler =  { "", "Bir", "İki", "Üç", "Dört", "Beş", "Altı",
                   "Yedi", "Sekiz", "Dokuz"};

        string[] onlar = { "", "On", "Yirmi", "Otuz", "Kırk", "Elli", "Altmış",
                   "Yetmiş", "Seksen", "Doksan"};

        string[] binler = { "Katrilyon", "Trilyon", "Milyar", "Milyon", "Bin", "" };

        int i;
        int[] bas = new int[3];
        string sonuç = "", ara_sonuç = "";
        int uz = sayı.Length;
        sayı = sayı.PadLeft(max_basamak_say, '0');
        for (i = 0; i <= max_basamak_say / 3 - 1; i++)
        {
            bas[0] = int.Parse(sayı.Substring(i * 3, 1));
            bas[1] = int.Parse(sayı.Substring(i * 3 + 1, 1));
            bas[2] = int.Parse(sayı.Substring(i * 3 + 2, 1));
            if (bas[0] == 0)
                ara_sonuç = "";
            else
                if (bas[0] == 1)
                ara_sonuç = "Yüz";
            else
                ara_sonuç = birler[bas[0]] + "Yüz ";
            ara_sonuç = ara_sonuç + onlar[bas[1]] + birler[bas[2]];
            if (ara_sonuç != "")
                ara_sonuç = ara_sonuç + binler[i];
            if (i > 1 && ara_sonuç == "BirBin")
                ara_sonuç = "Bin ";
            if (ara_sonuç != "")
                sonuç = string.Format("{0}{1}", sonuç, ara_sonuç);
        }
        if (sonuç.Trim() == "")
            sonuç = "Sıfır";
        return sonuç.Trim();
    }
    public static string SayiyiYaziyaCevirVirgullu(string Sayi, string ParaBirimi)
    {
        try
        {
            string OndalikParaBirimi = "";
            if (ParaBirimi.ToUpper() == "TRY") OndalikParaBirimi = "Kuruş";
            if (ParaBirimi.ToUpper() == "USD") OndalikParaBirimi = "Cent";
            if (ParaBirimi.ToUpper() == "Eur") OndalikParaBirimi = "Cent";


            string Sonuc = "";
            string OndalikSayi = "";
            if (Sayi == "") return Sonuc;
            decimal SayiyiYuvarla = Math.Round(Convert.ToDecimal(Sayi), 3);

            string[] split = SayiyiYuvarla.ToString().Split(new char[] { ',' });

            string TamSayi = split[0];
            if (split.Length > 1)
                OndalikSayi = split[1];

            TamSayi = TamSayi.Replace(".", "");

            string TamSayiSonuc = SayiyiYaziyaCevir(TamSayi);

            string OndalikSonuc = "";
            if (OndalikSayi != "")
            {
                if (Convert.ToInt32(OndalikSayi) > 0)
                    OndalikSonuc = SayiyiYaziyaCevir(OndalikSayi);
            }

            if (OndalikSonuc.Length > 0)
                Sonuc = string.Format("# {0} # {1} {2} {3}", TamSayiSonuc, ToTitleCase(ParaBirimi), OndalikSonuc, OndalikParaBirimi);
            else
                Sonuc = string.Format("# {0} # {1}", TamSayiSonuc, ToTitleCase(ParaBirimi));

            return Sonuc;
        }
        catch (Exception)
        {
            return "";
        }

    }

    public const string EmailTypeRegEx = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
    public const string TCKimlikTypeRegEx = @"^[1-9]{1}[0-9]{9}[02468]{1}$";
    public const string IbanNoTypeRegEx = @"^[A-Z]{2}[0-9]{2}[A-Za-z0-9]{22}$";
    public const string ConstNewRecordText = "< Yeni Kayıt >";
    public const string VergiNoTypeRegEx = @"/^[0-9]{10}$/";
    public const string TelefonNumarasiTypeRegEx = @"/^(05)([0-9]{2})\s?([0-9]{3})\s?([0-9]{2})\s?([0-9]{2})$/";

    public static ApplicationUser GetCurrentUser(IObjectSpace os)
    {
        return os.FindObject<ApplicationUser>(CriteriaOperator.Parse("Oid=CurrentUserId()"));
    }

    public static decimal Get_DovizKur(DovizTanim dovizTanim, DateTime kurTarihi)
    {
        decimal result = 0;
        Session session = dovizTanim.Session;
        CriteriaOperator criteria =
            CriteriaOperator.FromLambda<DovizGunlukKurD>(x => x.KurTarihi == kurTarihi
            && x.DovizTanim == dovizTanim);

        var KurList = session.FindObject<DovizGunlukKurD>(criteria);
        if (dovizTanim.DovizKodu == "TRY") result = 1;
        else if (KurList != null) result = KurList.DovizSatis;
        else result = 1;
        return result;
    }

    
    // Program.cs'de app build edildikten sonra enjekte edilir (statik Helper sınıfı DI alamadığı için köprü gerekli).
    public static IMemoryCache ImageCache { get; set; }

    public static byte[] GetImage(FileSystemStoreObject _File)
    {
        if (_File == null) return null;

        string cacheKey;
        try
        {
            // Anahtara dosyanın son değişiklik zamanı dahil edilir: resim değiştirildiğinde
            // anahtar otomatik değişir, bayat önbellek riski olmadan TTL tahmini gerekmez.
            cacheKey = $"Thumbnail:{_File.RealFileName}:{File.GetLastWriteTimeUtc(_File.RealFileName).Ticks}";
        }
        catch (Exception)
        {
            return null;
        }

        if (ImageCache != null && ImageCache.TryGetValue(cacheKey, out byte[] cached))
            return cached;

        byte[] image;
        try
        {
            image = File.ReadAllBytes(_File.RealFileName);
        }
        catch (Exception)
        {
            image = null;
        }

        if (ImageCache != null && image != null)
        {
            ImageCache.Set(cacheKey, image, new MemoryCacheEntryOptions
            {
                Size = image.Length,
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });
        }
        return image;
    }
}
