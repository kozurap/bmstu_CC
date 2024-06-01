import json
import re
from copy import deepcopy


class Grammar:
    def __init__(self):
        self.nonterms = set()
        self.terms = set()
        self.start_nonterm = None
        self.productions = {}

    @staticmethod
    def from_json(file_path):
        grammar = Grammar()

        file = open(file_path)
        doc = json.load(file)

        grammar.nonterms = set(doc["nonterms"])
        grammar.terms = set(doc["terms"])
        grammar.productions = doc["productions"]
        grammar.start_nonterm = doc["start_nonterm"]

        return grammar

    @staticmethod
    def remove_left_recursion(nonterm, productions):
        transit_nonterm = f"{nonterm}\'"
        transit_nonterm_productions = []

        new_productions = []
        for production in productions:
            if production == "eps":
                transit_nonterm_productions.append("eps")
                new_productions.append(transit_nonterm)
                continue

            match = re.match(f"{nonterm}(.*)", production)
            if match is not None:
                transit_nonterm_productions.append(f"{match.group(1)}{transit_nonterm}")
                continue

            new_productions.append(f"{production}{transit_nonterm}")

        return nonterm, new_productions, transit_nonterm, transit_nonterm_productions

    def closure(self, nonterm, prev_nonterm):
        productions_closure = []
        for production in self.productions[nonterm]:
            match = re.match(f"{prev_nonterm}(.*)", production)
            if match is not None:
                for prev_nonterm_production in self.productions[prev_nonterm]:
                    productions_closure.append(f"{prev_nonterm_production}{match.group(1)}")
                continue

            productions_closure.append(production)

        return productions_closure

    def removed_left_recursion(self):
        grammar = Grammar()
        grammar.terms = deepcopy(self.terms)
        grammar.start_nonterm = self.start_nonterm

        for _, nonterm in enumerate(reversed(sorted(self.nonterms))):
            grammar.nonterms.add(nonterm)
            grammar.productions[nonterm] = self.productions[nonterm]

            for _, prev_nonterm in enumerate(reversed(sorted(self.nonterms))):
                if nonterm == prev_nonterm:
                    break

                _, new_productions, transit_nonterm, transit_nonterm_productions = self.remove_left_recursion(
                    nonterm, self.closure(nonterm, prev_nonterm)
                )

                if len(transit_nonterm_productions) == 0:
                    continue

                grammar.nonterms.add(transit_nonterm)
                grammar.productions.update({nonterm: new_productions})
                grammar.productions.update({transit_nonterm: transit_nonterm_productions})

        return grammar

    @staticmethod
    def common_prefix(strings):
        return "".join([x[0] for x in zip(*strings) if x == (x[0],) * len(x)])

    def removed_unreachables(self):
        grammer = Grammar()
        grammer.start_nonterm = self.start_nonterm

        v = [set(self.start_nonterm)]

        while True:
            v.append(set(v[-1]))

            for symbol in self.nonterms | self.terms:
                for key in self.productions:
                    if key not in v[-2]:
                        continue

                    for rhs in self.productions[key]:
                        if re.search(f".*{re.escape(symbol)}.*", rhs):
                            v[-1].add(symbol)

            if v[-1] == v[-2]:
                break

        grammer.nonterms = self.nonterms & v[-1]
        grammer.terms = self.terms & v[-1]

        productions = {}
        for key in self.productions:
            if key in grammer.nonterms:
                productions[key] = self.productions[key]
        grammer.productions = productions

        return grammer

    def __str__(self):
        return \
            f"{{\n"\
            f"    {{{", ".join(sorted(self.nonterms))}}},\n"\
            f"    {{{", ".join(sorted(self.terms))}}},\n"\
            f"    {{\n        {",\n        ".join([f"{k} -> {" | ".join(self.productions[k])}" for k in sorted(self.productions.keys())])}\n    }},\n"\
            f"    {self.start_nonterm}\n"\
            f"}}"


def main():
    grammar = Grammar.from_json("grammar.json")
    print(f"input\n{grammar}", end="\n\n")
    grammar = grammar.removed_left_recursion()
    print(f"removed recursion\n{grammar}", end="\n\n")
    grammar = grammar.removed_unreachables()
    print(f"removed unreachables\n{grammar}")


if __name__ == "__main__":
    main()