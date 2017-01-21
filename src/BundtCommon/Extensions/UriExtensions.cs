using System;

namespace BundtBot.Extensions {
	public static class UriExtensions {
		public static Uri AddParameter(this Uri url, string paramName, string paramValue) {
			var uriBuilder = new UriBuilder(url);
			uriBuilder.Query += "&" + paramName + "=" + paramValue;
			return uriBuilder.Uri;
		}
	}
}
