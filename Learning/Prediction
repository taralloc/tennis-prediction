%Logistic regression
input = csvread('c:\users\francesco\desktop\x.csv');
[B, dev] = glmfit(input(:,2:end-1), [input(:,end) ones(length(input),1)], 'binomial', 'link', 'logit');
f = @(x) 1 ./ (1 + exp(-x));
lout = f(B(1)+input(:,2:end-1)*B(2:end));

%Test model
w = 0;
for i=1:length(lout)
    if lout(i) >= 0.5 && input(i,end) == 1
        w = w + 1;
    elseif lout(i) < 0.5 && input(i,end) == 0
        w = w + 1;
    end
end
disp(w/length(input));
