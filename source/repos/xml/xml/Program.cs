using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace xml
{
    class Program
    {
        static void Main(string[] args)
        {
            using (XmlWriter writer = XmlWriter.Create("books.xml"))
            {
                XmlDocument doc = new XmlDocument();
                writer.WriteStartElement("book");
                writer.WriteElementString("title", "Graphics Programming using GDI+");
                writer.WriteElementString("author", "Mahesh Chand");
                writer.WriteElementString("publisher", "Addison-Wesley");
                writer.WriteElementString("price", "64.95");
                writer.WriteEndElement();
                writer.Flush();
                string path = "C:\\test11.xml";
                //writer = new XmlTextWriter(path, null);
                doc.LoadXml(Convert.ToString(writer));
                doc.Save(writer);

            }
        }
    }
}
