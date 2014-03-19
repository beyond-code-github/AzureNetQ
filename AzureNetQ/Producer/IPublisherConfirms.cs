using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace AzureNetQ.Producer
{
    public interface IPublisherConfirms
    {
        Task PublishWithConfirm(IModel model, Action<IModel> publishAction);
    }
}