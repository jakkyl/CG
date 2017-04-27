import java.util.*;
import java.io.*;
import java.math.*;

class Solution {

    public static void main(String args[]) {
        Scanner in = new Scanner(System.in);
        int N = in.nextInt();
        Set<String> set = new HashSet<String>();
        
        for (int i = 0; i < N; i++) {
            String telephone = in.next();
            //System.err.println(telephone);
            for (int j = 0; j < telephone.length(); j++)
            {
                set.add(telephone.substring(0, j+1));
            }
            //System.err.println(set);
        }       
        System.out.println(set.size());
    }
}