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
        String filename = exchange.getIn().getHeader("CamelFileName", String.class);

        // OCR mit Tesseract ausführen (ORC = Optical Character Recognition)
        ProcessBuilder pb = new ProcessBuilder("tesseract", "input/" + filename, "stdout");
        pb.redirectErrorStream(true);
        Process process = pb.start();
        String text = new String(process.getInputStream().readAllBytes());

        // JSON für wish-processing-service erstellen
        Map<String, Object> jsonMap = new HashMap<>();
        jsonMap.put("filename", filename);
        jsonMap.put("extracted_text", text);

        // JSON an Python-Processing-Service senden
        String jsonOutput = new ObjectMapper().writeValueAsString(jsonMap);
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

    private void trySendWishFromImageRequest(String jsonPayload) {
        try (CloseableHttpClient client = HttpClients.createDefault()) {
            HttpPost httpPost = new HttpPost("http://wish-storage-service:8003/receive-image/");  // Fix: URL angepasst
            httpPost.setEntity(new StringEntity(jsonPayload));
            httpPost.setHeader("Content-Type", "application/json");

            client.execute(httpPost);
            System.out.println("OCR-Daten an wish-storage-service gesendet.");
        } catch (Exception e) {
            System.err.println("Fehler beim Senden an wish-storage-service: " + e.getMessage());
        }
    }
}