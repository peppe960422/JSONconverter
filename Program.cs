
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

Person p = new Person();
JSONSerializer j = new JSONSerializer();


string json = j.GetJson(p);

Console.WriteLine(json);


string m = "{\"name\":\"Giuseppe\",\"eta\":23,\"etwas\":[{\"citta\":\"Berlin\",\"via\":\"Schillerstrasse 1A\"},{\"citta\":\"Berlin\",\"via\":\"Schillerstrasse 1A\"},{\"citta\":\"Berlin\",\"via\":\"Schillerstrasse 1A\"}],\"indirizzo\":{\"citta\":\"Berlin\",\"via\":\"Schillerstrasse 1A\"}}";

Func<Person> fn = () => new Person();
Person c = j.CreateObject(m, fn);

Console.WriteLine(c.eta);
Console.WriteLine(c.name);
Console.WriteLine(c.indirizzo);

public class JSONSerializer {

    int findBrackletsIndex(char[] chars) 
    {
        int i = 0; 
        foreach (char c in chars)
        {

            if ( c == '{') { return i; }

            i++;

        }
        return i; 
    
    }

    public T CreateObject<T>(string json, Func<T> constructor)
    {
        T obj = constructor.Invoke();

        string innerJson = json.Trim();
        if (innerJson.StartsWith("{")) innerJson = innerJson.Substring(1);
        if (innerJson.EndsWith("}")) innerJson = innerJson.Substring(0, innerJson.Length - 1);

        var properties = SplitTopLevel(innerJson);

        foreach (string prop in properties)
        {
            string[] kv = prop.Split(new[] { ':' }, 2);
            if (kv.Length != 2) continue;

            string key = kv[0].Trim(' ', '"');
            string value = kv[1].Trim();

            PropertyInfo property = typeof(T).GetProperty(key);
            if (property == null || !property.CanWrite) continue;

            if (value.StartsWith("{"))
            {
            
                var method = typeof(JSONSerializer).GetMethod(nameof(CreateObject)).MakeGenericMethod(property.PropertyType);
                object nestedObj = method.Invoke(this, new object[] { value, Activator.CreateInstance(property.PropertyType) });
                property.SetValue(obj, nestedObj);
            }
            //else
            //{
            //    object convertedValue = Convert.ChangeType(value.Trim('"'), property.PropertyType);
            //    property.SetValue(obj, convertedValue);
            //}
        }

        return obj;
    }

    private List<string> SplitTopLevel(string json)
    {
        List<string> result = new();
        int braceCount = 0;
        int lastSplit = 0;

        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '{') braceCount++;
            if (json[i] == '}') braceCount--;
            if (json[i] == ',' && braceCount == 0)
            {
                result.Add(json.Substring(lastSplit, i - lastSplit));
                lastSplit = i + 1;
            }
        }

        result.Add(json.Substring(lastSplit)); 
        return result;
    }

 
    public string GetJson<T>(T obj)
    {
        var json = new StringBuilder();
        if (obj == null)
            return "null";
        else if (obj is string str)
        {
            json.Append($"\"{str}\"");
            return json.ToString();
        }
        else if (obj is int or double or bool)
        {
            json.Append($"{obj}");
            return json.ToString();
        }

        
        Type type = obj.GetType();

        json.Append("{");

        PropertyInfo[] properties = type.GetProperties();
        for (int i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];
            object value = prop.GetValue(obj);

            json.Append($"\"{prop.Name}\":");

            if (value == null)
            {
                json.Append("null");
            }
            else if (value is string str)
            {
                json.Append($"\"{str}\"");
            }
            else if (value is int or double or bool)
            {
                json.Append($"{value}");
            }
            else if (value is IList list)
            {
                json.Append("[");
                for (int j = 0; j < list.Count; j++)
                {
                    json.Append(GetJson(list[j]));
                    if (j < list.Count - 1)
                        json.Append(",");
                }
                json.Append("]");
            }
            else 
            {
                json.Append(GetJson(value));
            }

            if (i < properties.Length - 1)
                json.Append(",");
        }

        json.Append("}");
        return json.ToString();
    }
}
class Person
{
    public Person()
    {
            
    }
    public string name { get { return "Giuseppe"; } }
        public int eta {  get { return 23; } }

  //  public List<Adresse> etwas { get; set;  } =  new List<Adresse> { new Adresse(), new Adresse(), new Adresse() }; 
   public Adresse indirizzo {  get { return new Adresse(); } }
}

class Adresse
{
    public string citta { get { return "Berlin"; } }
    public string via {  get { return "Schillerstrasse 1A"; } }
}
