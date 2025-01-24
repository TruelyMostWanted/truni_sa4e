package org.example;

import org.apache.camel.CamelContext;
import org.apache.camel.impl.DefaultCamelContext;

public class Main {
    public static void main(String[] args) throws Exception {
        //(1) Create a new CamelContext
        CamelContext context = new DefaultCamelContext();

        //(2) Register the new route
        context.addRoutes(new CamelRoute());

        //(3) Start the context
        context.start();

        //(4) Print a "START" message to the console
        System.out.println("Microservice from Apache-Camel started!");
    }
}