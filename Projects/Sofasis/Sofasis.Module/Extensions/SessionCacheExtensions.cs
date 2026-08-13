/* ****************************************************************************
 * Proje     : Sofasis Erp Project
 * Dosya     : SessionCacheExtensions.cs
 * Açıklama  : Varsayılan (IsVarsayilan) ve tekil (singleton) parametre
 *             kayıtlarını Session ömrü boyunca önbelleğe alan yardımcı
 *             metotlar. Session GC olunca ilişkili önbellek girdisi de
 *             otomatik düşer (ConditionalWeakTable).
 * ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;

namespace Sofasis.Module.Extensions
{
    public static class SessionCacheExtensions
    {
        static readonly ConditionalWeakTable<Session, Dictionary<string, object>> caches = new ConditionalWeakTable<Session, Dictionary<string, object>>();

        // "IsVarsayilan" alanı işaretli tekil kaydı getirir (StokGrupTanim, BirimTanim, KDVTanim, DovizTanim, DepoTanim vb.).
        public static T GetVarsayilan<T>(this Session session) where T : class
        {
            Dictionary<string, object> cache = caches.GetOrCreateValue(session);
            string key = typeof(T).FullName;
            if (!cache.TryGetValue(key, out object value))
                cache[key] = value = session.FindObject<T>(new BinaryOperator("IsVarsayilan", true));
            return (T)value;
        }

        // Tablodaki tek satırı (StokParametre gibi genel ayar kayıtlarını) getirir.
        public static T GetSingleton<T>(this Session session) where T : class
        {
            Dictionary<string, object> cache = caches.GetOrCreateValue(session);
            string key = typeof(T).FullName + ":Singleton";
            if (!cache.TryGetValue(key, out object value))
                cache[key] = value = new XPCollection<T>(session).FirstOrDefault();
            return (T)value;
        }
    }
}
