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

public class CamelRoute extends RouteBuilder 
{
    private void processExchange(org.apache.camel.Exchange exchange) throws IOException
    {
		//(0) Define a new file name
        String filename = exchange.getIn().getHeader("CamelFileName", String.class);

		//(1) Use OCR (Optical Character Recognition) from Tesseract
        ProcessBuilder pb = new ProcessBuilder("tesseract", "input/" + filename, "stdout");
        pb.redirectErrorStream(true);
        Process process = pb.start();
        String text = new String(process.getInputStream().readAllBytes());

        //(2) Create a Map (as Json Object)
        Map<String, Object> jsonMap = new HashMap<>();
        jsonMap.put("FileName", filename);
        jsonMap.put("Description", text);

		//(3) Turn the Object into a JSON String
        String jsonOutput = new ObjectMapper().writeValueAsString(jsonMap);
		
		//(4) Send it to the serivce using the JSON string as body parameter
        trySendWishFromImageRequest(jsonOutput);
		
		//(5) Get the incoming request and add the JSON to the body
        exchange.getIn().setBody(jsonOutput);
    }

    @Override
    public void configure() 
	{
        from("file:input?noop=true")
            .process(this::processExchange)
            .log("Completed! JSON was successfully submitted!");

        from("timer:keepAlive?period=60000")
            .log("Running");
    }

    private void trySendWishFromImageRequest(String jsonPayload) 
	{
		//(1) Create a HTML Client that performs a POST Request 
        try (CloseableHttpClient client = HttpClients.createDefault()) 
		{
			//(2) Create a new POST Request object to our Requests API 
			String apiUrl = "http://172.18.0.10:8080/api/requests";
            HttpPost httpPost = new HttpPost(apiUrl);
			
			//(3) Set the Content-Type to JSON and set its data			
            httpPost.setEntity(new StringEntity(jsonPayload));
            httpPost.setHeader("Content-Type", "application/json");

			//(4) Perform the Request 
            client.execute(httpPost);
			
			//(5) Log to the console 
			System.out.println("Request sent!")
        } 
		catch (Exception e) 
		{
			//ALT: If a exception has been thrown, log it!
            System.err.println("ERROR sending POST-Request to the API: " + e.getMessage());
        }
    }
}