using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Web.Compilation;

namespace SqlResourcesNameSpace
{
    [
       DesignTimeResourceProviderFactory(typeof(SqlResourceDesignTimeFactory))
       ]
    public sealed class SqlResourceProviderFactory : ResourceProviderFactory
	{
		public SqlResourceProviderFactory()
		{
		}

		public override IResourceProvider CreateGlobalResourceProvider(string classKey)
		{
			return new SqlResourceProviderFactory.SqlResourceProvider(null, classKey);
		}

		public override IResourceProvider CreateLocalResourceProvider(string virtualPath)
		{
			virtualPath = Path.GetFileName(virtualPath);
			return new SqlResourceProviderFactory.SqlResourceProvider(virtualPath, null);
		}

		private sealed class SqlResourceProvider : IResourceProvider
		{
			private string _virtualPath;

			private string _className;

			private IDictionary _resourceCache;

			private static object CultureNeutralKey;

			IResourceReader System.Web.Compilation.IResourceProvider.ResourceReader
			{
				get
				{
					return new SqlResourceProviderFactory.SqlResourceReader(this.GetResourceCache(null));
				}
			}

			static SqlResourceProvider()
			{
				SqlResourceProviderFactory.SqlResourceProvider.CultureNeutralKey = new object();
			}

			public SqlResourceProvider(string virtualPath, string className)
			{
				this._virtualPath = virtualPath;
				this._className = className;
			}

			private IDictionary GetResourceCache(string cultureName)
			{
				object cultureNeutralKey;
				if (cultureName == null)
				{
					cultureNeutralKey = SqlResourceProviderFactory.SqlResourceProvider.CultureNeutralKey;
				}
				else
				{
					cultureNeutralKey = cultureName;
				}
				if (this._resourceCache == null)
				{
					this._resourceCache = new ListDictionary();
				}
				IDictionary item = this._resourceCache[cultureNeutralKey] as IDictionary;
				if (item == null)
				{
					item = SqlResourceHelper.GetResources(this._virtualPath, this._className, cultureName,  null);
					this._resourceCache[cultureNeutralKey] = item;
				}
				return item;
			}

			object System.Web.Compilation.IResourceProvider.GetObject(string resourceKey, CultureInfo culture)
			{
				string str = null;
				str = ((culture == null ? true : !(Convert.ToString(culture) != "")) ? CultureInfo.CurrentUICulture.Name : culture.Name);
				object item = this.GetResourceCache(str)[resourceKey];
				if (item == null)
				{
					this._resourceCache = null;
					this.GetResourceCache(str);
					item = this.GetResourceCache(str)[resourceKey];
				}
				if (item == null)
				{
					item = resourceKey;
				}
				return item;
			}
		}

		internal   sealed class SqlResourceReader : IResourceReader, IEnumerable, IDisposable
		{
			private IDictionary _resources;

			public SqlResourceReader(IDictionary resources)
			{
				this._resources = resources;
			}

			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this._resources.GetEnumerator();
			}

			void System.IDisposable.Dispose()
			{
			}

			void System.Resources.IResourceReader.Close()
			{
			}

			IDictionaryEnumerator System.Resources.IResourceReader.GetEnumerator()
			{
				return this._resources.GetEnumerator();
			}
		}
	}
}