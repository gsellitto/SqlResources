using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Web.Compilation;
using System.Web.UI.Design;
using System.ComponentModel.Design;
using System.Configuration;

namespace SqlResourcesNameSpace
{
    /// <summary>
    /// Factory per creare la classe SqlResourceDesignTimeProvider
    /// Per default il designer salva con la colture it-IT, questo parametro può essere modificato mettendo in AppSettings il valore LocalizationDefaultDesignCulture
    /// allacolture desiderata.
    /// La connection string da utilizzare per l'accesso al db può essere impostata con il valore LocalizationDatabasePath in Appsettings, oppure inserendo nelle connectionstring 
    /// la connessione con nome connectionstring_sql.
    /// </summary>

    public sealed class SqlResourceDesignTimeFactory : DesignTimeResourceProviderFactory
    {

        private IServiceProvider _serviceProvider = null;
        private SqlResourceDesignTimeProvider _localProvider = null;
        private SqlResourceDesignTimeProvider LocalProvider
        {
            get
            {
                if (_localProvider==null)
                {
                    _localProvider = new SqlResourceDesignTimeProvider(_serviceProvider);
                }
                return _localProvider;
            }
        }

        public override IResourceProvider CreateDesignTimeGlobalResourceProvider(IServiceProvider serviceProvider, string classKey)
        {
            return null;
        }

        public override IResourceProvider CreateDesignTimeLocalResourceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return LocalProvider;
        }

        public override IDesignTimeResourceWriter CreateDesignTimeLocalResourceWriter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return LocalProvider;
        }
    }



    /// <summary>
    /// Provider SQLResourceProvider
    /// 
    /// </summary>
    public sealed class SqlResourceDesignTimeProvider
        :IResourceProvider ,IDesignTimeResourceWriter  
    {
        private IDictionary _resourceCacheInternal;
        private IDictionary _resourceCache;
        private IServiceProvider _provider;               
        
                
        public SqlResourceDesignTimeProvider(IServiceProvider s)
        {
            _provider = s;
            _resourceCache = null;
            _resourceCacheInternal = null;
        }

        public  string DefaultDesignCulture()
        {
            if (_provider == null)
            {
                if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["LocalizationDefaultDesignCulture"]))
                {
                    return "it-IT";
                }
                else
                {
                    return ConfigurationManager.AppSettings["LocalizationDefaultDesignCulture"].ToString();
                }
            }
            else
            {
                IWebApplication webApp = (IWebApplication)_provider.GetService(typeof(IWebApplication));
                if (webApp.OpenWebConfiguration(true).AppSettings.Settings["LocalizationDefaultDesignCulture"] ==null )
                {
                    return "it-IT";
                }
                return webApp.OpenWebConfiguration(true).AppSettings.Settings["LocalizationDefaultDesignCulture"].ToString();
            }
        }
        private IDictionary ResourceCache
        {
            get
            {
                if (this._resourceCache == null)
                {
                    this._resourceCache = new ListDictionary();                    
                }
                return _resourceCache;
            }
        }

        private IDictionary Resource
        {
            get
            {
                if (this._resourceCacheInternal == null)
                {
                    //this._resourceCacheInternal = new ListDictionary();
                    this._resourceCacheInternal = SqlResourceHelper.GetResources(GetVirtualPath(_provider), null, DefaultDesignCulture(),  _provider);
                }
                return _resourceCacheInternal;
            }
        }

        IResourceReader System.Web.Compilation.IResourceProvider.ResourceReader
        {
            get
            {
                return new SqlResourceProviderFactory.SqlResourceReader(this.GetResourceCache(null));
            }
        }

        public void AddResource(string name, string value)
        {
            if (Resource.Contains(name))
                Resource[name] = value;
            else
                Resource.Add(name, value);
        }

        public void AddResource(string name, object value)
        {
            if (Resource.Contains(name))
                Resource[name] = value;
            else
                Resource.Add(name, value);
        }

        public void AddResource(string name, byte[] value)
        {
            if (Resource.Contains(name))
                Resource[name] = value;
            else
                Resource.Add(name, value);
        }

        public void Close()
        {            
        }

        public string CreateResourceKey(string resourceName, object obj)
        {
            int counter = 1;
            string ObjectTypeName = obj.GetType().Name;
            string KeyBaseName = ObjectTypeName + "Resource" + resourceName;
            counter = GetNextKeyIndex(KeyBaseName, counter);
            return string.Format("{0}{1}", KeyBaseName, counter);
        }

        private int GetNextKeyIndex(string key,int counter)
        {
            string tmp = string.Format("{0}{1}", key, counter);
            foreach(string k in Resource.Keys )
            {
                if (k.IndexOf(tmp)>=0)
                 return  GetNextKeyIndex(key, counter + 1);
            }
            return counter;
        }

        public void Dispose()
        {            
        }

        public void Generate()
        {
            string vPath = GetVirtualPath(_provider);
            //scrittua sul db
            var cache = Resource;
            foreach (object k in cache.Keys )
            {
                SqlResourceHelper.AddResource(vPath,string.Empty ,k.ToString(),cache[k].ToString(), DefaultDesignCulture(), _provider);
            }
        }

        private string GetVirtualPath(IServiceProvider provider)
        {
            IDesignerHost host = (IDesignerHost)provider.GetService(typeof(IDesignerHost));
            WebFormsRootDesigner rootDesigner = host.GetDesigner(host.RootComponent) as WebFormsRootDesigner;
            return System.IO.Path.GetFileName(rootDesigner.DocumentUrl);
        }

        public object GetObject(string resourceKey, CultureInfo culture)
        {
            string cultureName = string.Empty;
            IDictionary dict = GetResourceCache(cultureName);
            if (dict.Contains(resourceKey))
            {
                return dict[resourceKey];
            }
            else
            {
                dict = GetResourceCache(null);
                if (dict.Contains(resourceKey))
                {
                    return dict[resourceKey];
                }
            }
            return null;
        }

        private IDictionary GetResourceCache(string cultureName)
        {
            string cultureNeutralKey = cultureName == null ? DefaultDesignCulture() :cultureName ;            
            IDictionary item = ResourceCache[cultureNeutralKey] as IDictionary;
            if (item == null)
            {
                item = SqlResourceHelper.GetResources(GetVirtualPath(_provider ), null, cultureNeutralKey,  _provider );
                ResourceCache[cultureNeutralKey] = item;
            }
            return item;
        }
    }
}
