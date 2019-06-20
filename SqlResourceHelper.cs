using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.UI.Design;

namespace SqlResourcesNameSpace
{
    internal static class SqlResourceHelper
    {
        //nuova  mimmo
        //test di commento  jests
        private const string DatabaseLocationKey = "LocalizationDatabasePath";

        /// <summary>
        /// Ricava la connectionstring da utilizzare.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static string GetConnectionString(IServiceProvider provider)
        {
            if (provider == null)
            {
                if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[DatabaseLocationKey]))
                {
                    return ConfigurationManager.ConnectionStrings["connectionstring_sql"].ToString();
                }
                else
                {
                    return ConfigurationManager.AppSettings[DatabaseLocationKey].ToString();
                }
            }
            else
            {
                IWebApplication webApp = (IWebApplication)provider.GetService(typeof(IWebApplication));
                return webApp.OpenWebConfiguration(true).ConnectionStrings.ConnectionStrings["connectionstring_sql"].ToString();
            }
        }

        //riga di commento da portare
        //nuovo commento
        public static void AddResource(string virtualPath, string className, string resource_name, string value, string cultureName, IServiceProvider serviceProvider)
        {
            using (SqlConnection sqlConnection = new SqlConnection(GetConnectionString(serviceProvider)))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.Parameters.AddWithValue("@resource_object", string.IsNullOrEmpty(virtualPath) ? className : virtualPath);
                sqlCommand.Parameters.AddWithValue("@resource_name", resource_name);
                sqlCommand.Parameters.AddWithValue("@resource_value", value);
                sqlCommand.Parameters.AddWithValue("@culture_name", cultureName);
                sqlCommand.CommandText = "delete from ASPNET_GLOBALIZATION_RESOURCES where  resource_name=@resource_name and resource_object=@resource_object and culture_name=@culture_name";
                sqlCommand.ExecuteNonQuery();
                sqlCommand.CommandText = " insert into ASPNET_GLOBALIZATION_RESOURCES (resource_name ,resource_value,resource_object,culture_name ) values (@resource_name ,@resource_value,@resource_object,@culture_name) ";
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
        }

        /// riga da non portare
        /// llll
        /// lldkdkdk 
        /// <summary>
        /// Legge i valori dal db. VirtualPath è valorizzato nel caso di risorse locali, classnme è valorizzato nel caso di risorse globali.
        /// sono mutualmente esclusivi e vanno nella stessa colonna resource_object del db. Hanno peró due siginificati diversi.
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="className"></param>
        /// <param name="cultureName"></param>
        /// <param name="designMode"></param>
        /// <param name="serviceProvider">Serviceprovider serve per recuperare la connectionstring, per capire se si � in design mode o application mode.</param>
        /// <returns></returns>
        public static IDictionary GetResources(string virtualPath, string className, string cultureName, IServiceProvider serviceProvider)
        {
            using (SqlConnection sqlConnection = new SqlConnection(GetConnectionString(serviceProvider)))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                string resource_object = string.IsNullOrEmpty(virtualPath) ? className : virtualPath;
                if (string.IsNullOrEmpty(resource_object))
                {
                    throw new Exception("SqlResourceHelper.GetResources() - virtualPath or className missing from parameters.");
                }
                if (!string.IsNullOrEmpty(cultureName))
                {
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "select resource_name, resource_value from ASPNET_GLOBALIZATION_RESOURCES where resource_object = @resource_object and culture_name = @culture_name ";
                    sqlCommand.Parameters.AddWithValue("@resource_object", resource_object);
                    sqlCommand.Parameters.AddWithValue("@culture_name", cultureName);
                }
                else
                {
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = "select resource_name, resource_value from ASPNET_GLOBALIZATION_RESOURCES where resource_object = @resource_object and culture_name is null";
                    sqlCommand.Parameters.AddWithValue("@resource_object", resource_object);
                }                
                ListDictionary listDictionaries = new ListDictionary();                
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (sqlDataReader.Read())
                {
                    string str = sqlDataReader.GetString(sqlDataReader.GetOrdinal("resource_name"));
                    string str1 = sqlDataReader.GetString(sqlDataReader.GetOrdinal("resource_value"));
                    listDictionaries.Add(str, str1);
                }
                sqlConnection.Close();
                return listDictionaries;
            }
            
        }
    }
}