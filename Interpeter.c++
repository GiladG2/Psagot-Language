#include <iostream>
#include <string>
#include <exception>
#include <stdexcept>
#include <cstdlib>  // exit

using namespace std;

class Psagot {
public:
    static void main(int argc, char* args[]) {
        if (argc > 2) {
            throw runtime_error("Usage: Interpreter Sky");
        }
        if (argc == 1) {
            cout << "Running script: " << args[0] << endl;
        } else {
            cout << "Running interactive prompt..." << endl;
        }
    }
};

int main(int argc, char* argv[]) {
    try {
        Psagot::main(argc, argv);
    } catch (const exception& e) {
        cerr << e.what() << endl;
        exit(64); 
    }
    return 0;
}
