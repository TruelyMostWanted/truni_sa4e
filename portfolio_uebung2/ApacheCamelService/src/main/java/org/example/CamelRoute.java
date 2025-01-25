package org.example;

import org.apache.camel.builder.RouteBuilder;

import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import com.fasterxml.jackson.databind.ObjectMapper;

public class CamelRoute extends RouteBuilder {

    private void processExchange(org.apache.camel.Exchange exchange) throws IOException
    {
        //(1) Define the filename
        String filename = exchange.getIn().getHeader("CamelFileName", String.class);

        //(2) Perform OCR (Optical Character Recognition) with Tesseract
        ProcessBuilder pb = new ProcessBuilder("tesseract", "input/" + filename, "stdout");
        pb.redirectErrorStream(true);

        //(3) Start the process and read the output
        Process process = pb.start();
        String text = new String(process.getInputStream().readAllBytes());

        //(4) Create a JSON object
        Map<String, Object> jsonMap = new HashMap<>();
        jsonMap.put("FileName", filename);
        jsonMap.put("Description", text);

        //(5) Convert the JSON object to a string
        String jsonOutput = new ObjectMapper().writeValueAsString(jsonMap);

        //(6) Send the JSON object to the API
        trySendWishFromImageRequest(jsonOutput);
        exchange.getIn().setBody(jsonOutput);
    }

    @Override
    public void configure() {
        from("file:input?noop=true")
            .process(this::processExchange)
            .log("Completed! JSON was successfully submitted!");

        from("timer:keepAlive?period=60000")
                .log("Running");
    }

    private void trySendWishFromImageRequest(String jsonPayload)
    {
        //(1) Create a new HTTP client
        try (CloseableHttpClient client = HttpClients.createDefault())
        {
            //(2) Define the URL
            String postApiUrl = "http://172.19.0.10:8080/api/requests";

            //(3) Create a new HTTP POST request
            HttpPost httpPost = new HttpPost(postApiUrl);
            httpPost.setEntity(new StringEntity(jsonPayload));
            httpPost.setHeader("Content-Type", "application/json");

            //(4) Execute the request
            client.execute(httpPost);

            //(5) Print a success message to the Console
            System.out.println("Sending POST-Request to API using data loaded from file");
        }
        catch (Exception e)
        {
            //(6) On ERROR: Print an error message to the Console
            System.out.println("ERROR: Failed to send POST-Request to API: " + e.getMessage());
        }
    }
}