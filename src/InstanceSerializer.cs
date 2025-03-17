using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

public static class InstanceSerializer
{
    public static void Serialize<T>(T obj, string savePath, string fileName)
    {
        Directory.CreateDirectory(savePath);
        FileStream stream = new FileStream(savePath + fileName, FileMode.Create);
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        try
        {
            serializer.Serialize(stream, obj);
        }
        catch (SerializationException e)
        {
            throw e;
        }
        finally
        {
            stream.Close();
        }
    }

    public static T Deserialize<T>(string savePath, string fileName)
    {
        T loadedObj = default;

        FileStream stream;
        Directory.CreateDirectory(savePath);
        try
        {
            stream = new FileStream(savePath + fileName, FileMode.Open);
        }
        catch (FileNotFoundException)
        {
            return loadedObj;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(T));
        try
        {
            loadedObj = (T)serializer.Deserialize(stream);
        }
        catch (SerializationException e)
        {
            throw e;
        }
        finally
        {
            stream.Close();
        }
        return loadedObj;
    }
}