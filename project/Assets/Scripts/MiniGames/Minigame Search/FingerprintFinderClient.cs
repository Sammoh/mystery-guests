using UnityEngine;
using Unity.Netcode;

public class FingerprintFinderClient : NetworkBehaviour
{
    public FingerprintFinderServer server;

    void Start()
    {
        // Initialize server reference (for demonstration purposes; do this appropriately in your actual game)
        server = FindObjectOfType<FingerprintFinderServer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient && Input.GetKeyDown(KeyCode.Space))
        {
            // Simulate the action of finding a fingerprint
            FingerprintResult clientResult = FindFingerprint();

            // Notify the server
            server.ValidateFingerprintDiscoveryServerRpc(clientResult);
        }
    }

    private FingerprintResult FindFingerprint()
    {
        // Placeholder logic to simulate client-side action of finding a fingerprint
        return Random.Range(0, 2) == 0 ? FingerprintResult.Valid : FingerprintResult.Invalid;
    }
}