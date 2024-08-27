public class IDCounter {
    int counter = 0;

    public int GetID() {
        return counter++;
    }
}
