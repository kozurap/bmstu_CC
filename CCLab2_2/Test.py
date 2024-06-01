import unittest
from main import Grammar

class Test(unittest.TestCase):
    def setUp(self):
        self.grammar = Grammar.from_json("grammar.json")
        self.grammar2 = Grammar.from_json("grammar2.json")

    def test_remove_recursion1(self):
        test_grammar = self.grammar.removed_left_recursion()
        self.assertEqual(test_grammar.productions["A"], ["dA\'", "A\'"])
        self.assertEqual(test_grammar.productions["A\'"], ["cA\'", "eps"])\

    def test_remove_unreachables1(self):
        test_grammar = self.grammar.removed_unreachables()
        self.assertFalse(test_grammar.productions.keys().__contains__('Z'))

    def test_remove_recursion2(self):
        test_grammar = self.grammar2.removed_left_recursion()
        self.assertEqual(test_grammar.productions["A"], ["bcA\'", "fcA\'", "dA\'", "A\'"])
        self.assertEqual(test_grammar.productions["A\'"], ["acA\'", "eps"])
        self.assertEqual(test_grammar.productions["S"], ["Aa", "b", "f"])


    def test_remove_unreachables2(self):
        test_grammar = self.grammar2.removed_unreachables()
        self.assertFalse(test_grammar.productions.keys().__contains__('Z'))


if __name__ == "__main__":
  unittest.main()