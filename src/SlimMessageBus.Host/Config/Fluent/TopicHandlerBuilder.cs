using System;
using System.Linq;
using System.Threading.Tasks;

namespace SlimMessageBus.Host.Config
{
    public class TopicHandlerBuilder<TRequest, TResponse> : AbstractTopicConsumerBuilder
        where TRequest : IRequestMessage<TResponse>
    {
        public TopicHandlerBuilder(string topic, MessageBusSettings settings)
            : base(topic, typeof(TRequest), settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            ConsumerSettings.ResponseType = typeof(TResponse);

            var consumerSettingsExist = settings.Consumers.Any(x => x.Topic == topic && x.ConsumerMode == ConsumerMode.RequestResponse);
            Assert.IsFalse(consumerSettingsExist,
                () => new ConfigurationMessageBusException($"Attempted to configure request handler for topic '{topic}' when one was already configured. You can only have one request handler for a given topic, otherwise which response would you send back?"));
        }

        public TopicHandlerBuilder<TRequest, TResponse> WithHandler<THandler>()
            where THandler : IRequestHandler<TRequest, TResponse>
        {
            Assert.IsNotNull(ConsumerSettings.ResponseType,
                () => new ConfigurationMessageBusException($"The {nameof(ConsumerSettings)}.{nameof(ConsumerSettings.ResponseType)} is not set"));

            ConsumerSettings.ConsumerMode = ConsumerMode.RequestResponse;
            ConsumerSettings.ConsumerType = typeof(THandler);
            ConsumerSettings.ConsumerMethod = (consumer, message, name) => ((THandler)consumer).OnHandle((TRequest)message, name);
            ConsumerSettings.ConsumerMethodResult = (task) => ((Task<TResponse>)task).Result;

            return this;
        }

        public TopicHandlerBuilder<TRequest, TResponse> Instances(int numberOfInstances)
        {
            ConsumerSettings.Instances = numberOfInstances;
            return this;
        }
    }
}