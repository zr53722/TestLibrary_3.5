TestLibrary_3.5
===============

To solve the problem can not be measured in unit testing, simulation of most HTTP request context object


这个类库的由来，是为解决.net3.5下面做单元测试而产生的，在.NET3.5中，HttpContext对象都是无法进行测试的，
这个类模拟了其中大部分对象，使之能够进行单元测试。当然至少模拟了大部分，仍然有部分无法进行模拟，也不可能
进行模拟，比如Server.MapPath，特定的流操作等等。但能够解决大部分问题。


Author:zhou rui  周睿

236773862@qq.com
