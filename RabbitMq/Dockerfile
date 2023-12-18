FROM rabbitmq:3-management

COPY ./RabbitMq/enabled_plugins /etc/rabbitmq/enabled_plugins

RUN chown -R rabbitmq:rabbitmq /etc/rabbitmq/enabled_plugins

# What ports should be exposed?
EXPOSE 4369 5671 5672 15671 15672 15692 25672 35672-35682
