using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Genel Parametre Tanımlama")]
    public class GenelParametre : BaseClass
    {
        public GenelParametre(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                XPCollection<GenelParametre> entityList = new XPCollection<GenelParametre>(Session);
                int count = entityList.Count;
                if (count == 1)
                {
                    this.CancelEdit();
                    GenelParametre entity = entityList[0];
                    Session.DropChanges();
                    Session.Reload(entity);
                }
            }
        }

        OndalikBasamakSayisi miktarOndalikMaski = OndalikBasamakSayisi.Basamak4;
        OndalikBasamakSayisi tutarOndalikMaski = OndalikBasamakSayisi.Basamak2;

        [XafDisplayName("Miktar Ondalık Maskı")]
        public OndalikBasamakSayisi MiktarOndalikMaski
        {
            get => miktarOndalikMaski;
            set => SetPropertyValue(nameof(MiktarOndalikMaski), ref miktarOndalikMaski, value);
        }

        [XafDisplayName("Tutar Ondalık Maskı")]
        public OndalikBasamakSayisi TutarOndalikMaski
        {
            get => tutarOndalikMaski;
            set => SetPropertyValue(nameof(TutarOndalikMaski), ref tutarOndalikMaski, value);
        }
    }
}
