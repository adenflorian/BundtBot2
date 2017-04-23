using System.IO;
using Newtonsoft.Json;

namespace BundtBot.Extensions {
	public static class StringExtensions
    {
	    public static string Prettify(this string @this) {
			using (var stringReader = new StringReader(@this))
			using (var stringWriter = new StringWriter()) {
				var jsonReader = new JsonTextReader(stringReader);
				var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
				jsonWriter.WriteToken(jsonReader);
				return stringWriter.ToString();
			}
		}

	    public static bool IsNullOrWhiteSpace(this string @this) {
		    return string.IsNullOrWhiteSpace(@this);
	    }

		public static T Deserialize<T>(this string @this)
		{
			return JsonConvert.DeserializeObject<T>(@this);
		}

		public static bool DoesNotStartWith(this string @this, string value)
		{
			return @this.StartsWith(value) == false;
		}
	}
}
