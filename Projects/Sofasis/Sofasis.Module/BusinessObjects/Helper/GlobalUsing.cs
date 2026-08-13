using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class GlobalUsing
{


    public const string EmailTypeRegEx    = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
    public const string TCKimlikTypeRegEx = @"^[1-9]{1}[0-9]{9}[02468]{1}$";
    public const string IbanNoTypeRegEx   = @"^[A-Z]{2}[0-9]{2}[A-Za-z0-9]{22}$";

    public const string ConstNewRecordText = "< Yeni Kayıt >";

}
