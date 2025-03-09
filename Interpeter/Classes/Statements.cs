

public abstract class Statements{
 Expression expression;
}

public class Write:Statements{
    Expression expression;
    public Write (Expression expression){
        this.expression = expression;
    }
}