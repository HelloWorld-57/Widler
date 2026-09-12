package com.cinezy.keycloak.providers.events.kafka;

import org.keycloak.events.Event;
import org.keycloak.events.admin.AdminEvent;

import java.util.Arrays;
import java.util.Collections;
import java.util.Locale;
import java.util.Set;
import java.util.stream.Collectors;

public final class KafkaEventFilter {

    private final Set<String> allowedUserEvents;
    private final Set<String> allowedAdminEvents;

    public KafkaEventFilter(
            String userEvents,
            String adminEvents) {

        this.allowedUserEvents = parse(userEvents);
        this.allowedAdminEvents = parse(adminEvents);
    }

    public boolean isUserEventAllowed(Event event) {
        if (event == null || event.getType() == null) {
            return false;
        }

        return allowedUserEvents.contains(
                event.getType().name().toUpperCase(Locale.ROOT));
    }

    public boolean isAdminEventAllowed(AdminEvent event) {
        if (event == null
                || event.getOperationType() == null
                || event.getResourceType() == null) {
            return false;
        }

        String eventName =
                event.getOperationType().name()
                        + ":"
                        + event.getResourceType().name();

        return allowedAdminEvents.contains(
                eventName.toUpperCase(Locale.ROOT));
    }

    private static Set<String> parse(String value) {
        if (value == null || value.isBlank()) {
            return Collections.emptySet();
        }

        return Arrays.stream(value.split(","))
                .map(String::trim)
                .filter(s -> !s.isEmpty())
                .map(s -> s.toUpperCase(Locale.ROOT))
                .collect(Collectors.toUnmodifiableSet());
    }
}