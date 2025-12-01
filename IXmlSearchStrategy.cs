using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp3
{
    public interface IXmlSearchStrategy
    {
        List<Student> Search(string filePath, SearchCriteria criteria);
    }
}
