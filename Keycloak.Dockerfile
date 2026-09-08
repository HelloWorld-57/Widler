FROM quay.io/keycloak/keycloak:26.7.1

WORKDIR /opt/keycloak

COPY KeycloakProvider/keycloak-event-listener-kafka-0.0.3.jar \
     /opt/keycloak/providers/

RUN /opt/keycloak/bin/kc.sh build