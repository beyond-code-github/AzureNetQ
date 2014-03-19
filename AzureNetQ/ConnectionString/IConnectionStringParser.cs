using System;
using System.Linq;
using Sprache;

namespace AzureNetQ.ConnectionString
{
    public interface IConnectionStringParser
    {
        IConnectionConfiguration Parse(string connectionString);
    }

    public class ConnectionStringParser : IConnectionStringParser
    {
        public IConnectionConfiguration Parse(string connectionString)
        {
            try
            {
                var updater = ConnectionStringGrammar.ConnectionStringBuilder.Parse(connectionString);
                var connectionConfiguration = updater.Aggregate(new ConnectionConfiguration(), (current, updateFunction) => updateFunction(current));
                connectionConfiguration.Validate();
                return connectionConfiguration;
            }
            catch (ParseException parseException)
            {
                throw new AzureNetQException("Connection String {0}", parseException.Message);
            }
        }
    }
}