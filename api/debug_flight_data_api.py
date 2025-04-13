from flask import Flask, request

app = Flask(__name__)

@app.route("/debug", methods=["POST"])
def debug_log():
    data = request.get_json()
    
    # ✅ Print the received data
    print("\n📡 **Debug API Received Data:**")
    for feature, value in data.items():
        print(f"   - {feature}: {value}")

    return "Logged", 200  # Return success response

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001, debug=True)  # Debugging API
