namespace MediaFrontJapan.SCIP;

public interface IResponseParser<out T>
{
	T Parse(Response response);
}
