using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Models
{
    public enum DocumentType
    {
        Приход,
        Резерв,
        Расход
    }

    public enum DocumentStatus
    {
        Черновик,
        Зарезервировано,
        Списано,
        Оприходовано
    }
}
